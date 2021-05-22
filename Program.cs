using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using GetAgency;
using GetAgency.Data;
using HtmlAgilityPack;
using NetTopologySuite.Geometries;

namespace Getagency
{
    class Program
    {
        private const string URL = "https://www.banpara.b.br/macroscripts/Componentes/CanaisAtendimentoCascading.ashx";
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            var capitalCodes = await GetAgenciesCodes(true);
            var inlandCodes = await GetAgenciesCodes(false);

            var capitalAgencies = await GetAgenciesDetails(capitalCodes, true);
            var inlandAgencies = await GetAgenciesDetails(inlandCodes, false);

            SaveToDb(capitalAgencies, capitalCodes, true);
            SaveToDb(inlandAgencies, inlandCodes, false);
        }

        private static void SaveToDb(List<Agency> agencies, List<string> codes, bool isCapital)
        {
            foreach (var agency in agencies)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(agency.Info);

                var whatUrLookingFor = doc.GetElementbyId("info-point-map")
                    .SelectNodes("//div[@id='info-point-map']").ToArray().First();

                var props = new Dictionary<string, string>();
                foreach (var item in whatUrLookingFor.InnerHtml.Split("<br>"))
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        if (codes[agencies.IndexOf(agency)] == "98")
                            continue;

                        Console.WriteLine($"{codes[agencies.IndexOf(agency)]}");
                        Console.WriteLine(agency.Latitude);
                        Console.WriteLine(agency.Longitude);
                        foreach (var dic in props)
                        {
                            Console.WriteLine($"{dic.Key} - {dic.Value}");
                        }

                        if (Math.Abs(agency.Latitude) > 180 || Math.Abs(agency.Longitude) > 180)
                        {
                            agency.Latitude = 0;
                            agency.Longitude = 0;
                        }

                        string cep = "";
                        string telefone = "";
                        string bairro = "";
                        string horaIni = "";
                        string horaFim = "";
                        string autoIni = "";
                        string autoFim = "";

                        if (props.ContainsKey("Telefone"))
                            telefone = props["Telefone"].Substring(0, 13);

                        if (props.ContainsKey("Bairro"))
                            bairro = props["Bairro"];

                        if (props.ContainsKey("Cep"))
                            cep = props["Cep"];

                        if (props.ContainsKey("Horário de Atendimento"))
                        {
                            if (props["Horário de Atendimento"].Length >= 14)
                            {
                                horaIni = props["Horário de Atendimento"].Substring(0, 5);
                                horaFim = props["Horário de Atendimento"].Substring(9, 5);
                            }
                        }
                        else
                        {
                            horaIni = "10:00";
                            horaFim = "16:00";
                        }

                        if (props.ContainsKey("Horário de Auto - Atend."))
                        {
                            if (props["Horário de Auto - Atend."].Length >= 14)
                            {
                                autoIni = props["Horário de Auto - Atend."].Substring(0, 5);
                                autoFim = props["Horário de Auto - Atend."].Substring(9, 5);
                            }
                        }
                        else
                        {
                            autoIni = "08:00";
                            autoFim = "18:00";
                        }

                        using (var ctx = new AgencyDbContext())
                        {
                            ctx.Agencies.Add(new AgencyDB()
                            {
                                Id = Guid.NewGuid(),
                                Code = codes[agencies.IndexOf(agency)],
                                Name = props["Nome"],
                                Address = props["Endereço"],
                                Cep = cep,
                                City = props["Cidade"],
                                District = bairro,
                                IsCapital = isCapital,
                                IsStation = props["Nome"].StartsWith("PA"),
                                Location = new Point(agency.Latitude, agency.Longitude) { SRID = 4326 },
                                Phone = telefone,
                                ServiceStartTime = horaIni,
                                ServiceEndTime = horaFim,
                                SelfServiceStartTime = autoIni,
                                SelfServiceEndTime = autoFim,
                            });

                            ctx.SaveChanges();
                        }


                        props.Clear();
                        Console.WriteLine();
                        continue;
                    }
                    var startIndex = item.IndexOf("</b>");
                    props.Add(item.Substring(3, startIndex - 3).Replace(":", ""), item.Substring(startIndex + 5));
                }
            }
        }

        private static async Task<List<Agency>> GetAgenciesDetails(List<string> ids, bool isCapital)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                string url = "";

                if (isCapital)
                    url = URL + "?canal=AgencPosCap&nomes=[" +
                            $"{string.Join(",", ids.Select(n => n.ToString()))}]";
                else
                    url = URL + "?canal=AgencPosInter&nomes=[" +
                            $"{string.Join(",", ids.Select(n => n.ToString()))}]";


                // Console.WriteLine(url);

                var result = await client.GetStreamAsync(url);

                var agency = await JsonSerializer.DeserializeAsync<List<Agency>>(result);

                return agency;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine(hex.Message);
                throw;
            }
        }

        private static async Task<List<string>> GetAgenciesCodes(bool isCapital)
        {
            try
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json")
                );

                string url = "";

                if (isCapital)
                    url = URL + $"?canal=AgencPosCap&id=canais&val="
                                + "AgencPosCap&_=1621610035306";
                else
                    url = URL + "?canal=AgencPosInter&id=canais&val="
                                + "AgencPosInter&_=1621610076934";

                var resultJson = await client.GetStreamAsync(url);

                var result = await JsonSerializer.DeserializeAsync<AgencyCode>(resultJson);

                List<string> codes = new List<string>();
                foreach (var r in result.names)
                {
                    if (!string.IsNullOrEmpty(r.val))
                        codes.Add(r.val);
                }

                return codes;
            }
            catch (HttpRequestException hex)
            {
                Console.WriteLine(hex.Message);
                throw;
            }
        }
    }
}
