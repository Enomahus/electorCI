using System;
using System.Collections.Generic;
using System.Text;
using Application.Exceptions;
using Application.Interfaces.Services;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.RegistrationRequests.Common
{
    public class RegistrationService : IRegistrationService
    {
        public async Task<string> GenerateRequestReferenceAsync(
            WritableDbContext context,
            TimeProvider timeProvider,
            CancellationToken cancellationToken
        )
        {
            int year = timeProvider.GetUtcNow().Year;

            var lastRequestReference = await context
                .RegistrationRequests.Where(r => r.SubmissionDate.Year == year)
                .OrderByDescending(r => r.Reference)
                .Select(r => r.Reference)
                .FirstOrDefaultAsync(cancellationToken);

            long lastReference = 0;
            if (!string.IsNullOrEmpty(lastRequestReference) && lastRequestReference.StartsWith("DE"))
            {
                var sequenceNumericPart = lastRequestReference[8..];
                _ = long.TryParse(sequenceNumericPart, out lastReference);
            }

            var newReference = lastReference + 1;

            // Format final: DE-2025-0012547
            return $"DE-{year}-{newReference.ToString().PadLeft(7, '0')}";
        }

        public async Task<string> GenerateElectorNumberAsync(
            WritableDbContext context,
            long pollingStationId,
            CancellationToken cancellationToken
        )
        {
            var station =
                await context
                    .PollingStations.Include(p => p.District)
                    .FirstOrDefaultAsync(p => p.Id == pollingStationId, cancellationToken)
                ?? throw new NotFoundException(nameof(PollingStationDao), pollingStationId);

            string zoneCode = station.District.Code.PadLeft(5, '0');

            var lastElectorNumber = await context
                .Electors.AsNoTracking()
                .OrderByDescending(e => e.VoterRegistrationNumber)
                .Select(e => e.VoterRegistrationNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int sequenceNumber = 0;
            if (!string.IsNullOrEmpty(lastElectorNumber))
            {
                var seq = lastElectorNumber.Split(' ')[2];
                _ = int.TryParse(seq, out sequenceNumber);
            }

            string sequenceStr = sequenceNumber.ToString().PadLeft(6, '0');

            // 3. Calculer une clé de contrôle (Modulo 97) pour l'intégrité (ex: 11)
            // On concatène la zone et la séquence pour le calcul
            long rawNumber = long.Parse($"{zoneCode}{sequenceStr}");
            string checKey = (rawNumber % 97).ToString().PadLeft(2, '0');

            // 4. Formatge final: V 00034 006601 50
            return $"V {zoneCode} {sequenceStr} {checKey}";
        }
    }
}
