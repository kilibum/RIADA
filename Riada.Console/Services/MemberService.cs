using MySqlConnector;
using Riada.Console.DTOs;
using Riada.Domain.Entities.Membership;
using Riada.Domain.Enums;

namespace Riada.Console.Services;

/// <summary>
/// Service de gestion des membres (persistance MySQL, accès asynchrone).
/// </summary>
public class MemberService
{
    private readonly string _connectionString;

    public MemberService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // Règles métier appliquées : âge minimum 16 ans et email unique.
    public async Task<MemberDto> CreateMemberAsync(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        string? phone,
        string? city)
    {
        int age = CalculateAge(dateOfBirth);
        if (age < 16)
            throw new ArgumentException($"L'âge minimum requis est 16 ans. Âge calculé : {age} ans.");

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        // Vérifie l'unicité de l'email avant insertion.
        await using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM members WHERE email = @email";
            checkCmd.Parameters.AddWithValue("@email", email);
            if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                throw new InvalidOperationException($"L'adresse email '{email}' est déjà utilisée.");
        }

        await using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO members
                (first_name, last_name, email, date_of_birth, mobile_phone, address_city,
                 status, gdpr_consent_at, created_at, updated_at)
            VALUES
                (@firstName, @lastName, @email, @dateOfBirth, @phone, @city,
                 'active', NOW(3), NOW(3), NOW(3));
            SELECT LAST_INSERT_ID();";

        insertCmd.Parameters.AddWithValue("@firstName",   firstName);
        insertCmd.Parameters.AddWithValue("@lastName",    lastName);
        insertCmd.Parameters.AddWithValue("@email",       email);
        insertCmd.Parameters.AddWithValue("@dateOfBirth", dateOfBirth.ToString("yyyy-MM-dd"));
        insertCmd.Parameters.AddWithValue("@phone",       (object?)phone ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@city",        (object?)city ?? DBNull.Value);

        int newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

        var membre = Member.Create(firstName, lastName, email, dateOfBirth, phone, city);
        membre.AssignId(newId);

        return MapToDto(membre);
    }

    // Liste paginée des membres actifs, triés par nom.
    public async Task<List<MemberDto>> GetAllMembersAsync(int page, int pageSize)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, first_name, last_name, email, address_city, status,
                   total_visits, date_of_birth, created_at, updated_at
            FROM members
            WHERE status = 'active'
            ORDER BY last_name, first_name
            LIMIT @pageSize OFFSET @offset";

        cmd.Parameters.AddWithValue("@pageSize", pageSize);
        cmd.Parameters.AddWithValue("@offset",   (page - 1) * pageSize);

        var result = new List<MemberDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var membre = Member.Reconstitute(
                id:          reader.GetInt32("id"),
                firstName:   reader.GetString("first_name"),
                lastName:    reader.GetString("last_name"),
                email:       reader.GetString("email"),
                dateOfBirth: DateOnly.FromDateTime(reader.GetDateTime("date_of_birth")),
                city:        reader.IsDBNull(reader.GetOrdinal("address_city"))
                                 ? null
                                 : reader.GetString("address_city"),
                status:      Enum.Parse<MemberStatus>(reader.GetString("status"), ignoreCase: true),
                totalVisits: reader.GetInt32("total_visits"),
                createdAt:   reader.GetDateTime("created_at"),
                updatedAt:   reader.GetDateTime("updated_at")
            );

            result.Add(MapToDto(membre));
        }

        return result;
    }

    // Membres actifs proposés à la sélection (modification).
    public async Task<List<MemberOption>> GetSelectableMembersAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, first_name, last_name, address_city
            FROM   members
            WHERE  status = 'active'
            ORDER  BY last_name, first_name";

        var result = new List<MemberOption>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new MemberOption(
                reader.GetInt32("id"),
                $"{reader.GetString("first_name")} {reader.GetString("last_name")}",
                reader.IsDBNull(reader.GetOrdinal("address_city")) ? null : reader.GetString("address_city")));
        }
        return result;
    }

    // Coordonnées actuelles d'un membre, pour pré-remplir le formulaire de modification.
    public async Task<(string? Phone, string? City)?> GetMemberContactAsync(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT mobile_phone, address_city FROM members WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (
            reader.IsDBNull(reader.GetOrdinal("mobile_phone")) ? null : reader.GetString("mobile_phone"),
            reader.IsDBNull(reader.GetOrdinal("address_city")) ? null : reader.GetString("address_city"));
    }

    // Met à jour le téléphone et la ville d'un membre.
    public async Task<bool> UpdateMemberContactAsync(int id, string? phone, string? city)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE members
            SET    mobile_phone = @phone,
                   address_city = @city,
                   updated_at   = NOW(3)
            WHERE  id = @id";

        cmd.Parameters.AddWithValue("@phone", (object?)phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@city",  (object?)city ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id",    id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // Supprime un membre ET ses contrats associés, de façon atomique (transaction).
    public async Task<bool> DeleteMemberAsync(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        await using (var delContracts = conn.CreateCommand())
        {
            delContracts.Transaction = tx;
            delContracts.CommandText = "DELETE FROM contracts WHERE member_id = @id";
            delContracts.Parameters.AddWithValue("@id", id);
            await delContracts.ExecuteNonQueryAsync();
        }

        int affected;
        await using (var delMember = conn.CreateCommand())
        {
            delMember.Transaction = tx;
            delMember.CommandText = "DELETE FROM members WHERE id = @id";
            delMember.Parameters.AddWithValue("@id", id);
            affected = await delMember.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();
        return affected > 0;
    }

    private static MemberDto MapToDto(Member membre) => new MemberDto
    {
        Id          = membre.Id,
        FullName    = membre.FullName,
        Age         = membre.GetAge(),
        Email       = membre.Email,
        AddressCity = membre.AddressCity,
        Status      = membre.Status,
        TotalVisits = membre.TotalVisits,
        DisplayInfo = membre.GetDisplayInfo()
    };

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}
