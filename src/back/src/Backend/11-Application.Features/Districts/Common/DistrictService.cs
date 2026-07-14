using Application.Features.Common.District;
using Infrastructure.Persistence.SQLServer.Contexts;

namespace Application.Features.Districts.Common
{
    public class DistrictService(WritableDbContext context)
    {
        public async Task<long> CreateNewDistrictAsync(DistrictModel model, CancellationToken cancellationToken) {             
            
            var newDistrict = model.ToDao();
            context.Districts.Add(newDistrict);
            await context.SaveChangesAsync(cancellationToken);
            return newDistrict.Id;
        }
    }
}
