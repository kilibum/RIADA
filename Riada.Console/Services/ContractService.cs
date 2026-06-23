using MySqlConnector;
using Riada.Console.DTOs;
using Riada.Domain.Enums;
using Riada.Domain.Pricing;

namespace Riada.Console.Services;

/// <summary>
/// Service de gestion des contrats (persistance MySQL, accès asynchrone).
/// </summary>
public class ContractService
{
    private readonly string _connectionString;

    public ContractService(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Retourne le nom complet du membre actif, ou null s'il est introuvable.
    /// </summary>
    public async Task<string?> GetMemberFullNameAsync(int memberId)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT first_name, last_name FROM members WHERE id = @id AND status = 'active'";
        cmd.Parameters.AddWithValue("@id", memberId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return $"{reader.GetString("first_name")} {reader.GetString("last_name")}";
    }

    /// <summary>
    /// Retourne le nom et le prix de base du plan, ou null s'il est introuvable.
    /// </summary>
    public async Task<(string PlanName, decimal BasePrice)?> GetPlanInfoAsync(int planId)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT plan_name, base_price FROM subscription_plans WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", planId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return (reader.GetString("plan_name"), reader.GetDecimal("base_price"));
    }

    /// <summary>
    /// Retourne le nom du club, ou null s'il est introuvable.
    /// </summary>
    public async Task<string?> GetClubNameAsync(int clubId)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT name FROM clubs WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", clubId);

        return await cmd.ExecuteScalarAsync() is string name ? name : null;
    }

    // Le prix mensuel est calculé par la stratégie de tarification fournie.
    public async Task<ContractDto> CreateContractAsync(int memberId, int planId, int clubId, IPricingStrategy strategy)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        if (!await ExistsAsync(conn, "SELECT COUNT(*) FROM members WHERE id = @id AND status = 'active'", memberId))
            throw new InvalidOperationException($"Membre introuvable (ID : {memberId}).");

        var plan = await GetPlanDetailsAsync(conn, planId)
            ?? throw new InvalidOperationException($"Plan introuvable (ID : {planId}).");

        if (!await ExistsAsync(conn, "SELECT COUNT(*) FROM clubs WHERE id = @id", clubId))
            throw new InvalidOperationException($"Club introuvable (ID : {clubId}).");

        decimal prixMensuel = strategy.CalculateMonthly(plan.BasePrice);
        var dateDebut = DateOnly.FromDateTime(DateTime.UtcNow);
        var dateFin   = dateDebut.AddMonths(plan.CommitmentMonths);

        await using var insertCmd = conn.CreateCommand();
        insertCmd.CommandText = @"
            INSERT INTO contracts
                (member_id, plan_id, home_club_id, start_date, end_date,
                 contract_type, monthly_price, status, created_at, updated_at)
            VALUES
                (@memberId, @planId, @clubId, @startDate, @endDate,
                 'fixed_term', @monthlyPrice, 'active', NOW(3), NOW(3));
            SELECT LAST_INSERT_ID();";

        insertCmd.Parameters.AddWithValue("@memberId",     memberId);
        insertCmd.Parameters.AddWithValue("@planId",       planId);
        insertCmd.Parameters.AddWithValue("@clubId",       clubId);
        insertCmd.Parameters.AddWithValue("@startDate",    dateDebut.ToString("yyyy-MM-dd"));
        insertCmd.Parameters.AddWithValue("@endDate",      dateFin.ToString("yyyy-MM-dd"));
        insertCmd.Parameters.AddWithValue("@monthlyPrice", prixMensuel);

        int newId = Convert.ToInt32(await insertCmd.ExecuteScalarAsync());

        return new ContractDto
        {
            Id           = newId,
            MemberId     = memberId,
            PlanId       = planId,
            Status       = ContractStatus.Active,
            StartDate    = dateDebut,
            EndDate      = dateFin,
            MonthlyPrice = prixMensuel
        };
    }

    /// <summary>
    /// Annule un contrat en passant son statut à 'cancelled'.
    /// Retourne false si le contrat est introuvable ou déjà annulé.
    /// </summary>
    public async Task<bool> CancelContractAsync(int contractId)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE contracts
            SET    status       = 'cancelled',
                   cancelled_on = CURDATE(),
                   updated_at   = NOW(3)
            WHERE  id     = @id
              AND  status NOT IN ('cancelled', 'expired')";

        cmd.Parameters.AddWithValue("@id", contractId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // Change l'abonnement d'un contrat actif et son prix mensuel recalculé.
    // Retourne false si le contrat est introuvable ou inactif.
    public async Task<bool> ChangeContractPlanAsync(int contractId, int planId, decimal monthlyPrice)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        if (!await ExistsAsync(conn, "SELECT COUNT(*) FROM subscription_plans WHERE id = @id", planId))
            throw new InvalidOperationException($"Plan introuvable (ID : {planId}).");

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE contracts
            SET    plan_id       = @planId,
                   monthly_price = @monthlyPrice,
                   updated_at    = NOW(3)
            WHERE  id     = @id
              AND  status = 'active'";

        cmd.Parameters.AddWithValue("@planId",       planId);
        cmd.Parameters.AddWithValue("@monthlyPrice", monthlyPrice);
        cmd.Parameters.AddWithValue("@id",           contractId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    /// <summary>
    /// Retourne les contrats actifs les plus récents pour affichage.
    /// </summary>
    public async Task<List<ContractDto>> GetActiveContractsAsync(int maxCount = 5)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, member_id, plan_id, status, start_date, end_date, monthly_price
            FROM   contracts
            WHERE  status = 'active'
            ORDER  BY created_at DESC
            LIMIT  @maxCount";

        cmd.Parameters.AddWithValue("@maxCount", maxCount);

        var result = new List<ContractDto>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            result.Add(MapContract(reader));

        return result;
    }

    // Membres actifs proposés à la sélection lors de la création d'un contrat.
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

    // Plans d'abonnement proposés à la sélection.
    public async Task<List<PlanOption>> GetSelectablePlansAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, plan_name, base_price, commitment_months
            FROM   subscription_plans
            ORDER  BY base_price";

        var result = new List<PlanOption>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new PlanOption(
                reader.GetInt32("id"),
                reader.GetString("plan_name"),
                reader.GetDecimal("base_price"),
                reader.GetInt32("commitment_months")));
        }
        return result;
    }

    // Clubs proposés à la sélection.
    public async Task<List<ClubOption>> GetSelectableClubsAsync()
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, name, address_city
            FROM   clubs
            ORDER  BY name";

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

    // Tous les contrats (tous statuts), les plus récents d'abord.
    public async Task<List<ContractDto>> GetAllContractsAsync(int maxCount = 50)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, member_id, plan_id, status, start_date, end_date, monthly_price
            FROM   contracts
            ORDER  BY created_at DESC
            LIMIT  @maxCount";

        cmd.Parameters.AddWithValue("@maxCount", maxCount);

        var result = new List<ContractDto>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(MapContract(reader));

        return result;
    }

    // Mappe une ligne de contrat vers un ContractDto (prix mensuel à 0 si non renseigné).
    private static ContractDto MapContract(MySqlDataReader reader) => new ContractDto
    {
        Id        = reader.GetInt32("id"),
        MemberId  = reader.IsDBNull(reader.GetOrdinal("member_id")) ? 0 : reader.GetInt32("member_id"),
        PlanId    = reader.GetInt32("plan_id"),
        Status    = Enum.Parse<ContractStatus>(reader.GetString("status"), ignoreCase: true),
        StartDate = DateOnly.FromDateTime(reader.GetDateTime("start_date")),
        EndDate   = reader.IsDBNull(reader.GetOrdinal("end_date"))
                      ? null
                      : DateOnly.FromDateTime(reader.GetDateTime("end_date")),
        MonthlyPrice = reader.IsDBNull(reader.GetOrdinal("monthly_price")) ? 0m : reader.GetDecimal("monthly_price")
    };

    // Supprime définitivement un contrat.
    public async Task<bool> DeleteContractAsync(int contractId)
    {
        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM contracts WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", contractId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // ── Helpers privés ──────────────────────────────────────────────────────

    private static async Task<bool> ExistsAsync(MySqlConnection conn, string sql, int id)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    private record PlanDetails(string PlanName, decimal BasePrice, int CommitmentMonths);

    private static async Task<PlanDetails?> GetPlanDetailsAsync(MySqlConnection conn, int planId)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT plan_name, base_price, commitment_months FROM subscription_plans WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", planId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new PlanDetails(
            reader.GetString("plan_name"),
            reader.GetDecimal("base_price"),
            reader.GetInt32("commitment_months")
        );
    }
}
