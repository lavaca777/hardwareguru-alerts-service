namespace Alerts.Domain;

public static class AlertPolicy
{
    public const decimal ThresholdPercentage = 85m;

    public const string SuggestionText =
        "Haz undervolt o cambia a una fuente de mayor potencia.";

    /// <summary>
    /// Regla estricta: el uso debe superar el 85% para generar una alerta.
    /// Se compara en decimales para no depender del porcentaje redondeado.
    /// </summary>
    public static bool ShouldAlert(int estimatedConsumptionW, int psuPowerW)
        => psuPowerW > 0
           && estimatedConsumptionW >= 0
           && estimatedConsumptionW * 100m > psuPowerW * ThresholdPercentage;

    public static bool ShouldAlert(BuildSavedEvent buildEvent)
        => ShouldAlert(buildEvent.EstimatedConsumptionW, buildEvent.PsuPowerW);

    public static int CalculateUsagePercentage(int estimatedConsumptionW, int psuPowerW)
    {
        if (psuPowerW <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(psuPowerW), "La potencia de la fuente debe ser mayor que cero.");
        }

        if (estimatedConsumptionW < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(estimatedConsumptionW), "El consumo estimado no puede ser negativo.");
        }

        return (int)Math.Round(
            estimatedConsumptionW * 100m / psuPowerW,
            MidpointRounding.AwayFromZero);
    }

    public static string BuildMessage(int psuPowerW, int usagePercentage, string buildName)
        => $"Tu fuente de {psuPowerW} W está al {usagePercentage}% de uso con la build \"{buildName}\".";

    public static string BuildSuggestion() => SuggestionText;
}
