using MySqlConnector;
using Riada.Console.DTOs;
using Riada.Domain.Entities.ClubManagement;
using Riada.Domain.Enums;

namespace Riada.Console.Services;

/// <summary>
/// Service de gestion des employés (persistance MySQL, accès asynchrone).
/// </summary>
public class EmployeeService
{
    private readonly string _connectionString;

    public EmployeeService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    // Crée un employé après validation des invariants par l'entité (email unique).
    public async Task<EmployeeDto> CreateEmployeeAsync(
        string firstName, string lastName, string email, int clubId,
        EmployeeRole role, DateOnly hiredOn, decimal? monthlySalary, string? qualifications)
    {
        var employe = Employee.Create(clubId, firstName, lastName, email, role, hiredOn, monthlySalary, qualifications);

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using (var checkCmd = conn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM employees WHERE email = @email";
            checkCmd.Parameters.AddWithValue("@email", email);
            if (Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0)
                throw new InvalidOperationException($"L'adresse email '{email}' est déjà utilisée.");
        }

        await using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO employees
                (last_name, first_name, email, club_id, role,
                 monthly_salary, qualifications, hired_on, created_at, updated_at)
            VALUES
                (@lastName, @firstName, @email, @clubId, @role,
                 @salary, @qualifications, @hiredOn, NOW(3), NOW(3));
            SELECT LAST_INSERT_ID();";

        insertCmd.Parameters.AddWithValue("@lastName",       lastName);
        insertCmd.Parameters.AddWithValue("@firstName",      firstName);
        insertCmd.Parameters.AddWithValue("@email",          email);
        insertCmd.Parameters.AddWithValue("@clubId",         clubId);
        insertCmd.Parameters.AddWithValue("@role",           role.ToString().ToLowerInvariant());
        insertCmd.Parameters.AddWithValue("@salary",         (object?)monthlySalary ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@qualifications", (object?)qualifications ?? DBNull.Value);
        insertCmd.Parameters.AddWithValue("@hiredOn",        hiredOn.ToString("yyyy-MM-dd"));

        int newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
        employe.AssignId(newId);

        return MapToDto(employe, await GetClubNameAsync(conn, clubId) ?? "—");
    }

    // Liste les employés avec le nom de leur club, triés par nom.
    public async Task<List<EmployeeDto>> GetAllEmployeesAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT e.id, e.first_name, e.last_name, e.email, e.club_id,
                   e.role, e.monthly_salary, e.hired_on, c.name AS club_name
            FROM   employees e
            JOIN   clubs c ON c.id = e.club_id
            ORDER  BY e.last_name, e.first_name";

        var result = new List<EmployeeDto>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var role = Enum.Parse<EmployeeRole>(reader.GetString("role"), ignoreCase: true);
            var employe = Employee.Create(
                reader.GetInt32("club_id"),
                reader.GetString("first_name"),
                reader.GetString("last_name"),
                reader.GetString("email"),
                role,
                DateOnly.FromDateTime(reader.GetDateTime("hired_on")),
                reader.IsDBNull(reader.GetOrdinal("monthly_salary")) ? null : reader.GetDecimal("monthly_salary"));
            employe.AssignId(reader.GetInt32("id"));

            result.Add(MapToDto(employe, reader.GetString("club_name")));
        }
        return result;
    }

    // Clubs proposés à la sélection lors de la création d'un employé.
    public async Task<List<ClubOption>> GetSelectableClubsAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, name, address_city FROM clubs ORDER BY name";

        var result = new List<ClubOption>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new ClubOption(
                reader.GetInt32("id"),
                reader.GetString("name"),
                reader.GetString("address_city")));
        }
        return result;
    }

    // Champs complémentaires pour pré-remplir le formulaire de modification.
    public async Task<(decimal? Salary, string? Qualifications)?> GetEmployeeEditInfoAsync(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT monthly_salary, qualifications FROM employees WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (
            reader.IsDBNull(reader.GetOrdinal("monthly_salary")) ? null : reader.GetDecimal("monthly_salary"),
            reader.IsDBNull(reader.GetOrdinal("qualifications")) ? null : reader.GetString("qualifications"));
    }

    // Met à jour l'affectation et la rémunération d'un employé.
    public async Task<bool> UpdateEmployeeAsync(
        int id, int clubId, EmployeeRole role, decimal? monthlySalary, string? qualifications)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE employees
            SET    club_id        = @clubId,
                   role           = @role,
                   monthly_salary = @salary,
                   qualifications = @qualifications,
                   updated_at     = NOW(3)
            WHERE  id = @id";

        cmd.Parameters.AddWithValue("@clubId",         clubId);
        cmd.Parameters.AddWithValue("@role",           role.ToString().ToLowerInvariant());
        cmd.Parameters.AddWithValue("@salary",         (object?)monthlySalary ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@qualifications", (object?)qualifications ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@id",             id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // Supprime définitivement un employé.
    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM employees WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static async Task<string?> GetClubNameAsync(MySqlConnection conn, int clubId)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM clubs WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", clubId);
        return await cmd.ExecuteScalarAsync() is string name ? name : null;
    }

    private static EmployeeDto MapToDto(Employee employe, string clubName) => new EmployeeDto
    {
        Id            = employe.Id,
        FullName      = employe.FullName,
        Email         = employe.Email,
        Role          = employe.Role,
        ClubName      = clubName,
        MonthlySalary = employe.MonthlySalary,
        DisplayInfo   = employe.GetDisplayInfo()
    };
}
