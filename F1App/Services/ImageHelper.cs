using System.Collections.Generic;

namespace F1App.Services
{
    public static class ImageHelper
    {
        public static string GetDriverHeadshot(string code)
        {
            return code switch
            {
                "VER" => "https://media.formula1.com/content/dam/fom-website/drivers/M/MAXVER01_Max_Verstappen/maxver01.png",
                "PER" => "https://media.formula1.com/content/dam/fom-website/drivers/S/SERPER01_Sergio_Perez/serper01.png",
                "HAM" => "https://media.formula1.com/content/dam/fom-website/drivers/L/LEWHAM01_Lewis_Hamilton/lewham01.png",
                "RUS" => "https://media.formula1.com/content/dam/fom-website/drivers/G/GEORUS01_George_Russell/georus01.png",
                "LEC" => "https://media.formula1.com/content/dam/fom-website/drivers/C/CHALEC01_Charles_Leclerc/chalec01.png",
                "SAI" => "https://media.formula1.com/content/dam/fom-website/drivers/C/CARSAI01_Carlos_Sainz/carsai01.png",
                "NOR" => "https://media.formula1.com/content/dam/fom-website/drivers/L/LANNOR01_Lando_Norris/lannor01.png",
                "PIA" => "https://media.formula1.com/content/dam/fom-website/drivers/O/OSCPIA01_Oscar_Piastri/oscpia01.png",
                "ALO" => "https://media.formula1.com/content/dam/fom-website/drivers/F/FERALO01_Fernando_Alonso/feralo01.png",
                "STR" => "https://media.formula1.com/content/dam/fom-website/drivers/L/LANSTR01_Lance_Stroll/lanstr01.png",
                "OCO" => "https://media.formula1.com/content/dam/fom-website/drivers/E/ESTOCO01_Esteban_Ocon/estoco01.png",
                "GAS" => "https://media.formula1.com/content/dam/fom-website/drivers/P/PIEGAS01_Pierre_Gasly/piegas01.png",
                "ALB" => "https://media.formula1.com/content/dam/fom-website/drivers/A/ALEALB01_Alexander_Albon/alealb01.png",
                "SAR" => "https://media.formula1.com/content/dam/fom-website/drivers/L/LOGSAR01_Logan_Sargeant/logsar01.png",
                "RIC" => "https://media.formula1.com/content/dam/fom-website/drivers/D/DANRIC01_Daniel_Ricciardo/danric01.png",
                "TSU" => "https://media.formula1.com/content/dam/fom-website/drivers/Y/YUKTSU01_Yuki_Tsunoda/yuktsu01.png",
                "BOT" => "https://media.formula1.com/content/dam/fom-website/drivers/V/VALBOT01_Valtteri_Bottas/valbot01.png",
                "ZHO" => "https://media.formula1.com/content/dam/fom-website/drivers/G/GUAZHO01_Guanyu_Zhou/guazho01.png",
                "MAG" => "https://media.formula1.com/content/dam/fom-website/drivers/K/KEVMAG01_Kevin_Magnussen/kevmag01.png",
                "HUL" => "https://media.formula1.com/content/dam/fom-website/drivers/N/NICHUL01_Nico_Hulkenberg/nichul01.png",
                "LAW" => "https://media.formula1.com/content/dam/fom-website/drivers/L/LIALAW01_Liam_Lawson/lialaw01.png",
                "COL" => "https://media.formula1.com/content/dam/fom-website/drivers/F/FRACOL01_Franco_Colapinto/fracol01.png",
                "DOO" => "https://media.formula1.com/content/dam/fom-website/drivers/J/JACDOO01_Jack_Doohan/jacdoo01.png",
                _ => "https://www.formula1.com/content/dam/fom-website/drivers/2024/Drivers/placeholder.jpg.transform/2col/image.jpg"
            };
        }

        public static string GetTeamLogo(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return "";
            var name = teamName.ToLower();

            if (name.Contains("red bull") || name.Contains("oracle")) return "/img/teams/red-bull-racing.png";
            if (name.Contains("mercedes")) return "/img/teams/mercedes.png";
            if (name.Contains("ferrari")) return "/img/teams/ferrari.png";
            if (name.Contains("mclaren")) return "/img/teams/mclaren.png";
            if (name.Contains("aston martin")) return "/img/teams/aston-martin.png";
            if (name.Contains("alpine")) return "/img/teams/alpine.png";
            if (name.Contains("williams")) return "/img/teams/williams.png";
            if (name.Contains("rb") || name.Contains("racing bulls") || name.Contains("alphatauri")) return "/img/teams/rb.png";
            if (name.Contains("kick") || name.Contains("sauber") || name.Contains("alfa romeo") || name.Contains("stake")) return "/img/teams/kick-sauber.png";
            if (name.Contains("haas")) return "/img/teams/haas-f1-team.png";

            return "";
        }

        public static string GetNationality(string code)
        {
             return code switch
            {
                "VER" => "NED",
                "PER" => "MEX",
                "HAM" => "GBR",
                "RUS" => "GBR",
                "LEC" => "MON",
                "SAI" => "ESP",
                "NOR" => "GBR",
                "PIA" => "AUS",
                "ALO" => "ESP",
                "STR" => "CAN",
                "OCO" => "FRA",
                "GAS" => "FRA",
                "ALB" => "THA",
                "SAR" => "USA",
                "RIC" => "AUS",
                "TSU" => "JPN",
                "BOT" => "FIN",
                "ZHO" => "CHN",
                "MAG" => "DEN",
                "HUL" => "GER",
                "LAW" => "NZL",
                "COL" => "ARG",
                "DOO" => "AUS",
                _ => ""
            };
        }

        public static string GetCountryFlag(string countryName)
        {
            if (string.IsNullOrEmpty(countryName)) return "";
            var name = countryName.ToLower();

            // Handle codes directly
            if (name == "tha") return "/img/flags/thailand.png";
            if (name == "chn") return "/img/flags/china.png";
            if (name == "den") return "/img/flags/denmark.png";
            if (name == "ger") return "/img/flags/germany.png";
            if (name == "nzl") return "/img/flags/new-zealand.png";
            if (name == "arg") return "/img/flags/argentina.png";
            if (name == "fra") return "/img/flags/france.png";
            if (name == "fin") return "/img/flags/finland.png";
            if (name == "ned") return "/img/flags/netherlands.png";
            if (name == "mex") return "/img/flags/mexico.png";
            if (name == "gbr") return "/img/flags/great-britain.png";
            if (name == "mon") return "/img/flags/monaco.png";
            if (name == "esp") return "/img/flags/spain.png";
            if (name == "aus") return "/img/flags/australia.png";
            if (name == "can") return "/img/flags/canada.png";
            if (name == "usa") return "/img/flags/united-states.png";
            if (name == "jpn") return "/img/flags/japan.png";

            // Existing name-based checks
            if (name.Contains("bahrain")) return "/img/flags/bahrain.png";
            if (name.Contains("saudi")) return "/img/flags/saudi-arabia.png";
            if (name.Contains("australia")) return "/img/flags/australia.png";
            if (name.Contains("japan")) return "/img/flags/japan.png";
            if (name.Contains("china") || name.Contains("chinese")) return "/img/flags/china.png";
            if (name.Contains("miami")) return "/img/flags/united-states.png";
            if (name.Contains("emilia") || name.Contains("romagna")) return "/img/flags/italy.png";
            if (name.Contains("monaco")) return "/img/flags/monaco.png";
            if (name.Contains("canada") || name.Contains("canadian")) return "/img/flags/canada.png";
            if (name.Contains("spain") || name.Contains("spanish")) return "/img/flags/spain.png";
            if (name.Contains("austria")) return "/img/flags/austria.png";
            if (name.Contains("britain") || name.Contains("british")) return "/img/flags/great-britain.png";
            if (name.Contains("hungary") || name.Contains("hungarian")) return "/img/flags/hungary.png";
            if (name.Contains("belgium") || name.Contains("belgian")) return "/img/flags/belgium.png";
            if (name.Contains("netherlands") || name.Contains("dutch")) return "/img/flags/netherlands.png";
            if (name.Contains("italy") || name.Contains("italian")) return "/img/flags/italy.png";
            if (name.Contains("azerbaijan")) return "/img/flags/azerbaijan.png";
            if (name.Contains("singapore")) return "/img/flags/singapore.png";
            if (name.Contains("united states") || name.Contains("usa") || name.Contains("vegas")) return "/img/flags/united-states.png";
            if (name.Contains("mexico") || name.Contains("mexican")) return "/img/flags/mexico.png";
            if (name.Contains("brazil") || name.Contains("são paulo")) return "/img/flags/brazil.png";
            if (name.Contains("qatar")) return "/img/flags/qatar.png";
            if (name.Contains("abu dhabi")) return "/img/flags/abu-dhabi.png";

            return "";
        }

        public static string GetDriverTeam(string driverCode)
        {
            return driverCode.ToUpper() switch
            {
                "VER" => "Red Bull Racing",
                "PER" => "Red Bull Racing",
                "HAM" => "Mercedes",
                "RUS" => "Mercedes",
                "LEC" => "Ferrari",
                "SAI" => "Ferrari",
                "NOR" => "McLaren",
                "PIA" => "McLaren",
                "ALO" => "Aston Martin",
                "STR" => "Aston Martin",
                "GAS" => "Alpine",
                "OCO" => "Alpine",
                "ALB" => "Williams",
                "SAR" => "Williams",
                "TSU" => "RB",
                "RIC" => "RB",
                "BOT" => "Kick Sauber",
                "ZHO" => "Kick Sauber",
                "MAG" => "Haas F1 Team",
                "HUL" => "Haas F1 Team",
                "BEA" => "Ferrari",
                "COL" => "Williams",
                "LAW" => "RB",
                "DOO" => "Alpine",
                _ => "Unknown Team"
            };
        }
        public static string GetTeamColor(string? teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return "#333333";
            var name = teamName.ToLower();

            if (name.Contains("red bull") || name.Contains("oracle")) return "#3671C6";
            if (name.Contains("ferrari")) return "#E80020";
            if (name.Contains("mercedes")) return "#27F4D2";
            if (name.Contains("mclaren")) return "#FF8000";
            if (name.Contains("aston martin")) return "#229971";
            if (name.Contains("alpine")) return "#0093CC";
            if (name.Contains("williams")) return "#64C4FF";
            if (name.Contains("haas")) return "#B6BABD";
            if (name.Contains("kick") || name.Contains("sauber") || name.Contains("alfa romeo") || name.Contains("stake")) return "#52E252";
            if (name.Contains("rb") || name.Contains("racing bulls") || name.Contains("alphatauri")) return "#6692FF";

            return "#333333";
        }
    }
}
