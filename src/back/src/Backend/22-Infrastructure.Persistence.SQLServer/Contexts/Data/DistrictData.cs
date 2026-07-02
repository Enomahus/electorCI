using Application.Common.Enums;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.SQLServer.Contexts.Data
{
    public static class DistrictData
    {
        public static IEnumerable<DistrictDao> GetDistrictData =>
            [
                // 01. District autonome d'Abidjan
                new DistrictDao
                {
                    Id = 1,
                    Code = "001",
                    Wording = "District autonome d'Abidjan",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 2,
                    Code = "001001",
                    ParentId = 1,
                    Wording = "Abidjan",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 02. Agnéby-Tiassa
                new DistrictDao
                {
                    Id = 3,
                    Code = "002",
                    Wording = "Agnéby-Tiassa",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 4,
                    Code = "002001",
                    ParentId = 3,
                    Wording = "Agboville",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 5,
                    Code = "002002",
                    ParentId = 3,
                    Wording = "Sikensi",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 6,
                    Code = "002003",
                    ParentId = 3,
                    Wording = "Taabo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 7,
                    Code = "002004",
                    ParentId = 3,
                    Wording = "Tiassalé",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 03. Bafing
                new DistrictDao
                {
                    Id = 8,
                    Code = "003",
                    Wording = "Bafing",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 9,
                    Code = "003001",
                    ParentId = 8,
                    Wording = "Koro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 10,
                    Code = "003002",
                    ParentId = 8,
                    Wording = "Ouaninou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 11,
                    Code = "003003",
                    ParentId = 8,
                    Wording = "Touba",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 04. Bagoué
                new DistrictDao
                {
                    Id = 12,
                    Code = "004",
                    Wording = "Bagoué",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 13,
                    Code = "004001",
                    ParentId = 12,
                    Wording = "Boundiali",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 14,
                    Code = "004002",
                    ParentId = 12,
                    Wording = "Kouto",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 15,
                    Code = "004003",
                    ParentId = 12,
                    Wording = "Tengréla",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 05. Bélier
                new DistrictDao
                {
                    Id = 16,
                    Code = "005",
                    Wording = "Bélier",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 17,
                    Code = "005001",
                    ParentId = 16,
                    Wording = "Didiévi",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 18,
                    Code = "005002",
                    ParentId = 16,
                    Wording = "Djékanou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 19,
                    Code = "005003",
                    ParentId = 16,
                    Wording = "Tiébissou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 20,
                    Code = "005004",
                    ParentId = 16,
                    Wording = "Toumodi",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 06. Béré
                new DistrictDao
                {
                    Id = 21,
                    Code = "006",
                    Wording = "Béré",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 22,
                    Code = "006001",
                    ParentId = 21,
                    Wording = "Dianra",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 23,
                    Code = "006002",
                    ParentId = 21,
                    Wording = "Kounahiri",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 24,
                    Code = "006003",
                    ParentId = 21,
                    Wording = "Mankono",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 07. Bounkani
                new DistrictDao
                {
                    Id = 25,
                    Code = "007",
                    Wording = "Bounkani",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 26,
                    Code = "007001",
                    ParentId = 25,
                    Wording = "Bouna",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 27,
                    Code = "007002",
                    ParentId = 25,
                    Wording = "Doropo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 28,
                    Code = "007003",
                    ParentId = 25,
                    Wording = "Nassian",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 29,
                    Code = "007004",
                    ParentId = 25,
                    Wording = "Téhini",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 08. Cavally
                new DistrictDao
                {
                    Id = 30,
                    Code = "008",
                    Wording = "Cavally",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 31,
                    Code = "008001",
                    ParentId = 30,
                    Wording = "Bloléquin",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 32,
                    Code = "008002",
                    ParentId = 30,
                    Wording = "Guiglo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 33,
                    Code = "008003",
                    ParentId = 30,
                    Wording = "Taï",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 34,
                    Code = "008004",
                    ParentId = 30,
                    Wording = "Toulepleu",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 09. Folon
                new DistrictDao
                {
                    Id = 35,
                    Code = "009",
                    Wording = "Folon",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 36,
                    Code = "009001",
                    ParentId = 35,
                    Wording = "Kaniasso",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 37,
                    Code = "009002",
                    ParentId = 35,
                    Wording = "Minignan",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 10. Gbêkê
                new DistrictDao
                {
                    Id = 38,
                    Code = "010",
                    Wording = "Gbêkê",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 39,
                    Code = "010001",
                    ParentId = 38,
                    Wording = "Béoumi",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 40,
                    Code = "010002",
                    ParentId = 38,
                    Wording = "Botro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 41,
                    Code = "010003",
                    ParentId = 38,
                    Wording = "Bouaké",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 42,
                    Code = "010004",
                    ParentId = 38,
                    Wording = "Sakassou",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 11. Gbôklé
                new DistrictDao
                {
                    Id = 43,
                    Code = "011",
                    Wording = "Gbôklé",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 44,
                    Code = "011001",
                    ParentId = 43,
                    Wording = "Fresco",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 45,
                    Code = "011002",
                    ParentId = 43,
                    Wording = "Sassandra",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 12. Gôh
                new DistrictDao
                {
                    Id = 46,
                    Code = "012",
                    Wording = "Gôh",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 47,
                    Code = "012001",
                    ParentId = 46,
                    Wording = "Gagnoa",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 48,
                    Code = "012002",
                    ParentId = 46,
                    Wording = "Oumé",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 13. Gontougou
                new DistrictDao
                {
                    Id = 49,
                    Code = "013",
                    Wording = "Gontougou",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 50,
                    Code = "013001",
                    ParentId = 49,
                    Wording = "Bondoukou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 51,
                    Code = "013002",
                    ParentId = 49,
                    Wording = "Koun-Fao",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 52,
                    Code = "013003",
                    ParentId = 49,
                    Wording = "Sandégué",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 53,
                    Code = "013004",
                    ParentId = 49,
                    Wording = "Tanda",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 54,
                    Code = "013005",
                    ParentId = 49,
                    Wording = "Transua",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 14. Grands Ponts
                new DistrictDao
                {
                    Id = 55,
                    Code = "014",
                    Wording = "Grands Ponts",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 56,
                    Code = "014001",
                    ParentId = 55,
                    Wording = "Dabou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 57,
                    Code = "014002",
                    ParentId = 55,
                    Wording = "Grand-Lahou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 58,
                    Code = "014003",
                    ParentId = 55,
                    Wording = "Jacqueville",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 15. Guémon
                new DistrictDao
                {
                    Id = 59,
                    Code = "015",
                    Wording = "Guémon",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 60,
                    Code = "015001",
                    ParentId = 59,
                    Wording = "Bangolo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 61,
                    Code = "015002",
                    ParentId = 59,
                    Wording = "Duékoué",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 62,
                    Code = "015003",
                    ParentId = 59,
                    Wording = "Facobly",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 63,
                    Code = "015004",
                    ParentId = 59,
                    Wording = "Kouibly",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 16. Haut-Sassandra
                new DistrictDao
                {
                    Id = 64,
                    Code = "016",
                    Wording = "Haut-Sassandra",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 65,
                    Code = "016001",
                    ParentId = 64,
                    Wording = "Daloa",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 66,
                    Code = "016002",
                    ParentId = 64,
                    Wording = "Issia",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 67,
                    Code = "016003",
                    ParentId = 64,
                    Wording = "Vavoua",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 68,
                    Code = "016004",
                    ParentId = 64,
                    Wording = "Zoukougbeu",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 17. Iffou
                new DistrictDao
                {
                    Id = 69,
                    Code = "017",
                    Wording = "Iffou",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 70,
                    Code = "017001",
                    ParentId = 69,
                    Wording = "Daoukro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 71,
                    Code = "017002",
                    ParentId = 69,
                    Wording = "M’Bahiakro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 72,
                    Code = "017003",
                    ParentId = 69,
                    Wording = "Ouellé",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 73,
                    Code = "017004",
                    ParentId = 69,
                    Wording = "Prikro",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 18. Indénié-Djuablin
                new DistrictDao
                {
                    Id = 74,
                    Code = "018",
                    Wording = "Indénié-Djuablin",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 75,
                    Code = "018001",
                    ParentId = 74,
                    Wording = "Abengourou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 76,
                    Code = "018002",
                    ParentId = 74,
                    Wording = "Agnibilékrou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 77,
                    Code = "018003",
                    ParentId = 74,
                    Wording = "Bettié",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 19. Kabadougou
                new DistrictDao
                {
                    Id = 78,
                    Code = "019",
                    Wording = "Kabadougou",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 79,
                    Code = "019001",
                    ParentId = 78,
                    Wording = "Gbéléban",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 80,
                    Code = "019002",
                    ParentId = 78,
                    Wording = "Madinani",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 81,
                    Code = "019003",
                    ParentId = 78,
                    Wording = "Odienné",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 82,
                    Code = "019004",
                    ParentId = 78,
                    Wording = "Samatiguila",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 83,
                    Code = "019005",
                    ParentId = 78,
                    Wording = "Séguélon",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 20. La Mé
                new DistrictDao
                {
                    Id = 84,
                    Code = "020",
                    Wording = "La Mé",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 85,
                    Code = "020001",
                    ParentId = 84,
                    Wording = "Adzopé",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 86,
                    Code = "020002",
                    ParentId = 84,
                    Wording = "Akoupé",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 87,
                    Code = "020003",
                    ParentId = 84,
                    Wording = "Alépé",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 88,
                    Code = "020004",
                    ParentId = 84,
                    Wording = "Yakassé-Attobrou",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 21. Lôh-Djiboua
                new DistrictDao
                {
                    Id = 89,
                    Code = "021",
                    Wording = "Lôh-Djiboua",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 90,
                    Code = "021001",
                    ParentId = 89,
                    Wording = "Divo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 91,
                    Code = "021002",
                    ParentId = 89,
                    Wording = "Guitry",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 92,
                    Code = "021003",
                    ParentId = 89,
                    Wording = "Lakota",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 22. Marahoué
                new DistrictDao
                {
                    Id = 93,
                    Code = "022",
                    Wording = "Marahoué",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 94,
                    Code = "022001",
                    ParentId = 93,
                    Wording = "Bonon",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 95,
                    Code = "022002",
                    ParentId = 93,
                    Wording = "Bouaflé",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 96,
                    Code = "022003",
                    ParentId = 93,
                    Wording = "Gohitafla",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 97,
                    Code = "022004",
                    ParentId = 93,
                    Wording = "Sinfra",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 98,
                    Code = "022005",
                    ParentId = 93,
                    Wording = "Zuénoula",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 23. Hambol
                new DistrictDao
                {
                    Id = 99,
                    Code = "023",
                    Wording = "Hambol",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 100,
                    Code = "023001",
                    ParentId = 99,
                    Wording = "Dabakala",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 101,
                    Code = "023002",
                    ParentId = 99,
                    Wording = "Katiola",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 102,
                    Code = "023003",
                    ParentId = 99,
                    Wording = "Niakaramadougou",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 24. Moronou
                new DistrictDao
                {
                    Id = 103,
                    Code = "024",
                    Wording = "Moronou",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 104,
                    Code = "024001",
                    ParentId = 103,
                    Wording = "Arrah",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 105,
                    Code = "024002",
                    ParentId = 103,
                    Wording = "Bongouanou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 106,
                    Code = "024003",
                    ParentId = 103,
                    Wording = "M’Batto",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 25. Nawa
                new DistrictDao
                {
                    Id = 107,
                    Code = "025",
                    Wording = "Nawa",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 108,
                    Code = "025001",
                    ParentId = 107,
                    Wording = "Buyo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 109,
                    Code = "025002",
                    ParentId = 107,
                    Wording = "Guéyo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 110,
                    Code = "025003",
                    ParentId = 107,
                    Wording = "Méagui",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 111,
                    Code = "025004",
                    ParentId = 107,
                    Wording = "Soubré",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 26. N'Zi
                new DistrictDao
                {
                    Id = 112,
                    Code = "026",
                    Wording = "N'Zi",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 113,
                    Code = "026001",
                    ParentId = 112,
                    Wording = "Bocanda",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 114,
                    Code = "026002",
                    ParentId = 112,
                    Wording = "Dimbokro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 115,
                    Code = "026003",
                    ParentId = 112,
                    Wording = "Kouassi-Kouassikro",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 27. Poro
                new DistrictDao
                {
                    Id = 116,
                    Code = "027",
                    Wording = "Poro",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 117,
                    Code = "027001",
                    ParentId = 116,
                    Wording = "Dikodougou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 118,
                    Code = "027002",
                    ParentId = 116,
                    Wording = "Korhogo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 119,
                    Code = "027003",
                    ParentId = 116,
                    Wording = "M’Bengué",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 120,
                    Code = "027004",
                    ParentId = 116,
                    Wording = "Sinématiali",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 28. San-Pédro
                new DistrictDao
                {
                    Id = 121,
                    Code = "028",
                    Wording = "San-Pédro",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 122,
                    Code = "028001",
                    ParentId = 121,
                    Wording = "San-Pédro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 123,
                    Code = "028002",
                    ParentId = 121,
                    Wording = "Tabou",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 29. Sud-Comoé
                new DistrictDao
                {
                    Id = 124,
                    Code = "029",
                    Wording = "Sud-Comoé",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 125,
                    Code = "029001",
                    ParentId = 124,
                    Wording = "Aboisso",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 126,
                    Code = "029002",
                    ParentId = 124,
                    Wording = "Adiaké",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 127,
                    Code = "029003",
                    ParentId = 124,
                    Wording = "Grand-Bassam",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 128,
                    Code = "029004",
                    ParentId = 124,
                    Wording = "Tiapoum",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 30. Tchologo
                new DistrictDao
                {
                    Id = 129,
                    Code = "030",
                    Wording = "Tchologo",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 130,
                    Code = "030001",
                    ParentId = 129,
                    Wording = "Ferkessédougou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 131,
                    Code = "030002",
                    ParentId = 129,
                    Wording = "Kong",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 132,
                    Code = "030003",
                    ParentId = 129,
                    Wording = "Ouangolodougou",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 31. Tonkpi
                new DistrictDao
                {
                    Id = 133,
                    Code = "031",
                    Wording = "Tonkpi",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 134,
                    Code = "031001",
                    ParentId = 133,
                    Wording = "Biankouma",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 135,
                    Code = "031002",
                    ParentId = 133,
                    Wording = "Danané",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 136,
                    Code = "031003",
                    ParentId = 133,
                    Wording = "Man",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 137,
                    Code = "031004",
                    ParentId = 133,
                    Wording = "Sipilou",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 138,
                    Code = "031005",
                    ParentId = 133,
                    Wording = "Zouan-Hounien",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 32. Worodougou
                new DistrictDao
                {
                    Id = 139,
                    Code = "032",
                    Wording = "Worodougou",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 140,
                    Code = "032001",
                    ParentId = 139,
                    Wording = "Kani",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 141,
                    Code = "032002",
                    ParentId = 139,
                    Wording = "Séguéla",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 33. District autonome de Yamoussoukro
                new DistrictDao
                {
                    Id = 142,
                    Code = "033",
                    Wording = "District autonome de Yamoussoukro",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 143,
                    Code = "033001",
                    ParentId = 142,
                    Wording = "Attiégouakro",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 144,
                    Code = "033002",
                    ParentId = 142,
                    Wording = "Yamoussoukro",
                    Level = ElectoralDistrictLevel.Department,
                },
                // 34. Diaspora
                new DistrictDao
                {
                    Id = 145,
                    Code = "099",
                    Wording = "Diaspora",
                    Level = ElectoralDistrictLevel.Region,
                },
                new DistrictDao
                {
                    Id = 146,
                    Code = "005001001",
                    Wording = "Boli",
                    ParentId = 17,
                    Level = ElectoralDistrictLevel.SubPrefecture,
                },
                new DistrictDao
                {
                    Id = 147,
                    Code = "005001002",
                    Wording = "Didiévi",
                    ParentId = 17,
                    Level = ElectoralDistrictLevel.SubPrefecture,
                },
                new DistrictDao
                {
                    Id = 148,
                    Code = "005001003",
                    Wording = "Molonou-blé",
                    ParentId = 17,
                    Level = ElectoralDistrictLevel.SubPrefecture,
                },
                new DistrictDao
                {
                    Id = 149,
                    Code = "005001004",
                    Wording = "Raviart",
                    ParentId = 17,
                    Level = ElectoralDistrictLevel.SubPrefecture,
                },
                new DistrictDao
                {
                    Id = 150,
                    Code = "005001005",
                    Wording = "Tié-N'diekro",
                    ParentId = 17,
                    Level = ElectoralDistrictLevel.SubPrefecture,
                },
                new DistrictDao
                {
                    Id = 151,
                    Code = "005001001001",
                    Wording = "Boli",
                    ParentId = 146,
                    Level = ElectoralDistrictLevel.Municipality,
                },
                new DistrictDao
                {
                    Id = 152,
                    Code = "005001001001001",
                    Wording = "EPP Allanikro",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 153,
                    Code = "005001001001002",
                    Wording = "EPP Anokoi-Kouamekro",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 154,
                    Code = "005001001001003",
                    Wording = "EPP Labo",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 155,
                    Code = "005001001001004",
                    Wording = "EPP Adjebo",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 156,
                    Code = "005001001001005",
                    Wording = "EPP Takikro",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 157,
                    Code = "005001001001006",
                    Wording = "EPP Aka Kouamekro",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 158,
                    Code = "005001001001007",
                    Wording = "EPP Kongobo",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 159,
                    Code = "005001001001008",
                    Wording = "EPP Boli 1",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 160,
                    Code = "005001001001009",
                    Wording = "EPP Boli 3",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 161,
                    Code = "005001001001010",
                    Wording = "EPP Yoboueplissou",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 162,
                    Code = "005001001001011",
                    Wording = "EPP Anokoi-Djezou",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 163,
                    Code = "005001001001012",
                    Wording = "EPP Grodiekro",
                    ParentId = 151,
                    Level = ElectoralDistrictLevel.VotingLocation,
                },
                new DistrictDao
                {
                    Id = 164,
                    Code = "099001",
                    Wording = "Afrique du Sud",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 165,
                    Code = "099002",
                    Wording = "Allemagne",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 166,
                    Code = "099003",
                    Wording = "Angola",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 167,
                    Code = "099004",
                    Wording = "Belgique",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 168,
                    Code = "099005",
                    Wording = "Benin",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 169,
                    Code = "099006",
                    Wording = "Burkina Fasso",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 170,
                    Code = "099007",
                    Wording = "Canada",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 171,
                    Code = "099008",
                    Wording = "Congo",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 172,
                    Code = "099009",
                    Wording = "Espagne",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 173,
                    Code = "099010",
                    Wording = "Etats-Unis",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 174,
                    Code = "099011",
                    Wording = "France",
                    ParentId = 145,
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 175,
                    Code = "099012",
                    ParentId = 145,
                    Wording = "Gabon",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 176,
                    Code = "099013",
                    ParentId = 145,
                    Wording = "Ghana",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 177,
                    Code = "099014",
                    ParentId = 145,
                    Wording = "Grande-Bretagne",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 178,
                    Code = "099015",
                    ParentId = 145,
                    Wording = "Guinée",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 179,
                    Code = "099016",
                    ParentId = 145,
                    Wording = "Italie",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 180,
                    Code = "099017",
                    ParentId = 145,
                    Wording = "Mali",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 181,
                    Code = "099018",
                    ParentId = 145,
                    Wording = "Maroc",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 182,
                    Code = "099019",
                    ParentId = 145,
                    Wording = "Mauritanie",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 183,
                    Code = "099020",
                    ParentId = 145,
                    Wording = "Senegal",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 184,
                    Code = "099021",
                    ParentId = 145,
                    Wording = "Suisse",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 185,
                    Code = "099022",
                    ParentId = 145,
                    Wording = "Togo",
                    Level = ElectoralDistrictLevel.Department,
                },
                new DistrictDao
                {
                    Id = 186,
                    Code = "099023",
                    ParentId = 145,
                    Wording = "Tunisie",
                    Level = ElectoralDistrictLevel.Department,
                },
            ];
    }
}
