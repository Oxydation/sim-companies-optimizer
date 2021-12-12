using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using SimCompaniesOptimizer.Interfaces;
using SimCompaniesOptimizer.Models.ExchangeTracker;

namespace SimCompaniesOptimizer.Utils;

public class ExchangeTrackerReader : IExchangeTrackerReader
{
    public async Task<IEnumerable<ExchangeTrackerEntry>> GetAllEntriesFromExchangeApiAsync(
        CancellationToken cancellationToken)
    {
        const string uri = $"{SimCompaniesConstants.ExchangeApiCsvDownloadUrl}";
        using var client = new HttpClient();
        var getResponse = client.GetAsync(uri, cancellationToken).Result;
        getResponse.EnsureSuccessStatusCode();
        var contentStream = await getResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var streamReader = new StreamReader(contentStream);
        return ReadAllEntriesFromCsvToMemory(streamReader);
    }

    private IEnumerable<ExchangeTrackerEntry> ReadAllEntriesFromCsvToMemory(TextReader streamReader)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = ",",
            IgnoreBlankLines = true,
            DetectColumnCountChanges = true
        };

        using var csv = new CsvReader(streamReader, config);
        // Skip 7 lines
        for (var i = 0; i < 7; i++) csv.Read();

        var result = new List<ExchangeTrackerEntry>();
        while (csv.Read())
        {
            if (csv.Context.Reader[2].Contains("DAILY AVERAGE")) return result;

            result.Add(csv.GetRecord<ExchangeTrackerEntry>());
        }

        return result;
        // return csv.GetRecords<ExchangeTrackerEntry>().ToList();
    }
}