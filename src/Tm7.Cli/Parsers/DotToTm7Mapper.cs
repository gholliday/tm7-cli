namespace Tm7.Cli.Parsers;

public record EntityTypeMapping(string TypeId, string GenericTypeId);

public static class DotToTm7Mapper
{
    /// <summary>
    /// Determines the TM7 type and generic type for an entity based on its label and boundary membership.
    /// </summary>
    public static EntityTypeMapping MapEntityType(string id, string label, bool isInsideBoundary)
    {
        var upper = label.ToUpperInvariant();
        var idUpper = id.ToUpperInvariant();

        // Key Vault
        if (upper.Contains("KEY VAULT"))
            return new("SE.DS.TMCore.AzureKeyVault", "GE.DS");

        // Blob / Storage
        if (upper.Contains("BLOB") || upper.Contains("STORAGE"))
            return new("SE.DS.TMCore.AzureStorage", "GE.DS");

        // Postgres
        if (upper.Contains("POSTGRES"))
            return new("SE.DS.TMCore.AzurePostgresDB", "GE.DS");

        // Cosmos
        if (upper.Contains("COSMOS") || idUpper == "COSMOS")
            return new("SE.P.TMCore.AzureDocumentDB", "GE.DS");

        // Azure Data Explorer / Kusto / ADX
        if (upper.Contains("DATA EXPLORER") || upper.Contains("KUSTO") || upper.Contains("ADX"))
            return new("SE.P.TMCore.ADE", "GE.P");

        // App Insights - no specific TMT type, use generic process
        if (upper.Contains("APP INSIGHTS") || upper.Contains("APPLICATION INSIGHTS") || idUpper == "APPINSIGHTS")
            return new("GE.P", "GE.P");

        // Entra / Azure AD / AAD
        if (upper.Contains("ENTRA") || upper.Contains("AZURE AD") || upper.Contains("AAD") || idUpper == "AAD")
            return new("SE.P.TMCore.AzureAD", "GE.P");

        // Front Door
        if (upper.Contains("FRONT DOOR"))
            return new("GE.P", "GE.P");

        // Web App / Node.js
        if (upper.Contains("WEB APP") || upper.Contains("NODE.JS") || upper.Contains("NODEJS"))
            return new("SE.P.TMCore.AzureAppServiceWebApp", "GE.P");

        // cronjob / Web Job
        if (upper.Contains("CRONJOB") || upper.Contains("CRON JOB") || upper.Contains("WEB JOB") || upper.Contains("WEBJOB"))
            return new("SE.P.TMCore.AzureWebJob", "GE.P");

        // MS Graph / Directory
        if (upper.Contains("GRAPH") || upper.Contains("DIRECTORY"))
            return new("GE.P", "GE.P");

        // Browser
        if (upper.Contains("BROWSER"))
            return new("SE.EI.TMCore.Browser", "GE.EI");

        // External interactor if outside boundary
        if (!isInsideBoundary)
            return new("GE.EI", "GE.EI");

        // Default: generic process inside boundary
        return new("GE.P", "GE.P");
    }

    /// <summary>
    /// Maps a boundary to its TM7 type.
    /// </summary>
    public static EntityTypeMapping MapBoundaryType(string label)
    {
        if (label.ToUpperInvariant().Contains("AZURE"))
            return new("SE.TB.TMCore.AzureTrustBoundary", "GE.TB.B");
        return new("GE.TB.B", "GE.TB.B");
    }

    /// <summary>
    /// Classifies entities inside a boundary into layout columns.
    /// Column 0: External-facing processes (x=300) - Front Door, Entra, Graph
    /// Column 1: Core processes (x=500) - cronjobs, Web App
    /// Column 2: Data stores and supporting processes (x=750) - blobs, cosmos, postgres, key vault, ADX, App Insights
    /// </summary>
    public static int GetLayoutColumn(string id, string label, string genericTypeId)
    {
        var upper = label.ToUpperInvariant();
        var idUpper = id.ToUpperInvariant();

        // Data stores always go right
        if (genericTypeId == "GE.DS")
            return 2;

        // ADX, App Insights go right
        if (upper.Contains("DATA EXPLORER") || upper.Contains("KUSTO") || upper.Contains("ADX") ||
            upper.Contains("APP INSIGHTS") || upper.Contains("APPLICATION INSIGHTS") || idUpper == "APPINSIGHTS")
            return 2;

        // Front Door, Entra/AAD, Graph/Directory go left
        if (upper.Contains("FRONT DOOR") || upper.Contains("ENTRA") || upper.Contains("AZURE AD") ||
            upper.Contains("AAD") || idUpper == "AAD" ||
            upper.Contains("GRAPH") || upper.Contains("DIRECTORY"))
            return 0;

        // Core processes (cronjobs, web app) go center
        return 1;
    }
}
