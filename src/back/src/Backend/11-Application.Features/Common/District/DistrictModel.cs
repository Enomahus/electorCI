using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Application.Common.Enums;
using Google.Apis.PeopleService.v1.Data;
using Infrastructure.Persistence.Entities;
using Microsoft.Graph.Models;

namespace Application.Features.Common.District
{
    public class DistrictModel
    {
        public string? Code { get; set; }
        public string? Wording { get; set; }
        public ElectoralDistrictLevel Level { get; set; }
        public long? ParentId { get; set; }
        public bool IsActive { get; set; }

        public DistrictDao ToDao()
        {
            return new DistrictDao()
            {
                Code = Code,
                Wording = Wording,
                Level = Level,
                ParentId = ParentId,
            };
        }

        public static DistrictModel FromDao(DistrictDao dao, DateTimeOffset now)
        {
            return new DistrictModel()
            {
                Code = dao.Code,
                Wording = dao.Wording,
                Level = dao.Level,
                ParentId = dao.ParentId,
                IsActive = dao.DisabledDate == null || dao.DisabledDate > now,
            };
        }
    }
}
