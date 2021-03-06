﻿// Developed by: Gabriel Duarte
// 
// Created at: 26/07/2015 15:54

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Autofac;
using Ionic.Zip;
using MediaManager.Model;
using MediaManager.Properties;
using MediaManager.Services;

namespace MediaManager.Helpers
{
    public class APIRequests
    {
        public static async Task<bool> GetAtualizacoes()
        {
            if (Settings.Default.API_UltimaDataAtualizacaoTVDB == default(DateTime))
            {
                Settings.Default.API_UltimaDataAtualizacaoTVDB = DateTime.Now.AddDays(-5);
                Settings.Default.Save();
            }

            DateTime dataAtualizacao = DateTime.Now;
            int dias = (Settings.Default.API_UltimaDataAtualizacaoTVDB - dataAtualizacao).Days;
            string url = Settings.Default.API_UrlTheTVDB + "/api/" + Settings.Default.API_KeyTheTVDB + "/updates/";
            var nomeArquivo = "updates_";
            string xmlString = null;
            // RandomNum para não dar problema ao excluir um arquivo ainda sendo utilizado.
            int randomNum = new Random().Next(1, 55555);
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                       Settings.Default.AppName, "Metadata", "temp" + randomNum, "updatesTemp.zip");

            if (dias == 0)
            {
                nomeArquivo += "day";
            }
            else if (dias < 0 && dias > -7)
            {
                nomeArquivo += "week";
            }
            else if (dias <= -7 && dias > -30)
            {
                nomeArquivo += "month";
            }
            else
            {
                nomeArquivo += "all";
            }
            url += nomeArquivo + ".zip";

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                using (var wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(new Uri(url), path);
                }

                using (ZipFile zip = ZipFile.Read(path))
                {
                    ZipEntry xmlFileEntry = zip[nomeArquivo + ".xml"];
                    using (var ms = new MemoryStream())
                    {
                        xmlFileEntry.Extract(ms);
                        var sr = new StreamReader(ms);
                        ms.Position = 0;
                        xmlString = sr.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.Delete(Path.GetDirectoryName(path));
                }
            }
            var xml = new XmlDocument();

            xml.LoadXml(xmlString);

            XmlNodeList nodesSeries = xml.SelectNodes("/Data/Series");
            XmlNodeList nodesEpisodios = xml.SelectNodes("/Data/Episode");
            XmlNodeList nodesBanners = xml.SelectNodes("/Data/Banner");

            var seriesService = App.Container.Resolve<SeriesService>();
            var episodiosService = App.Container.Resolve<EpisodiosService>();

            List<Serie> lstSeriesAnimes = seriesService.GetSeriesEAnimes();
            List<Episodio> lstEpisodios = episodiosService.GetLista();

            foreach (XmlNode item in nodesSeries)
            {
                if (lstSeriesAnimes.Select(x => x.nCdApi.ToString()).Contains(item.SelectSingleNode("id").InnerText))
                {
                    var nCdApi = 0;
                    int.TryParse(item.SelectSingleNode("id").InnerText, out nCdApi);

                    Serie serieDB = seriesService.GetPorCodigoApi(nCdApi);

                    //data.Series[0].Episodes = new List<Episode>(data.Episodes);

                    if (int.Parse(serieDB.sNrUltimaAtualizacao) < int.Parse(item.SelectSingleNode("time").InnerText))
                    {
                        Serie serie = await GetSerieInfoAsync(nCdApi, Settings.Default.pref_IdiomaPesquisa);

                        serie.nCdVideo = serieDB.nCdVideo;
                        serie.sDsPasta = serieDB.sDsPasta;
                        serie.bFlAnime = serieDB.bFlAnime;
                        serie.nIdTipoConteudo = serieDB.nIdTipoConteudo;
                        serie.sDsTitulo = serieDB.sDsTitulo;
                        serie.sAliases = serieDB.sAliases;

                        await seriesService.UpdateAsync(serie);
                    }
                }
            }

            foreach (XmlNode item in nodesEpisodios)
            {
                var nCdApi = 0;
                int.TryParse(item.SelectSingleNode("id").InnerText, out nCdApi);
                if (lstEpisodios.Select(x => x.nCdEpisodioAPI.ToString()).Contains(nCdApi + ""))
                {
                    Episodio episodioDB = episodiosService.GetPorCodigoAPI(nCdApi);

                    if (int.Parse(episodioDB.sNrUltimaAtualizacao) < int.Parse(item.SelectSingleNode("time").InnerText))
                    {
                        Episodio episodio = await GetEpisodeInfoAsync(nCdApi, Settings.Default.pref_IdiomaPesquisa);
                        episodio.nCdEpisodio = episodioDB.nCdEpisodio;
                        episodiosService.Update(episodio);
                    }
                }
                else if (
                    lstSeriesAnimes.Select(x => x.nCdApi.ToString())
                                   .Contains(item.SelectSingleNode("Series").InnerText))
                {
                    Episodio episodio = await GetEpisodeInfoAsync(nCdApi, Settings.Default.pref_IdiomaPesquisa);
                    episodiosService.Adicionar(episodio);
                }
            }

            foreach (XmlNode item in nodesBanners)
            {
                if (item.SelectSingleNode("type").InnerText == Enums.TipoImagem.Fanart.ToString().ToLower() ||
                    item.SelectSingleNode("type").InnerText == Enums.TipoImagem.Poster.ToString().ToLower())
                {
                    if (
                        lstSeriesAnimes.Select(x => x.nCdApi.ToString())
                                       .Contains(item.SelectSingleNode("Series").InnerText))
                    {
                        int IDApi = int.Parse(item.SelectSingleNode("Series").InnerText);
                        string urlImagem = Settings.Default.API_UrlTheTVDB + "/banners/" +
                                           item.SelectSingleNode("path").InnerText;
                        string tipo = item.SelectSingleNode("type").InnerText;
                        if (tipo == Enums.TipoImagem.Fanart.ToString().ToLower())
                        {
                            using (var db = new Context())
                            {
                                Serie serie = (from series in db.Serie
                                               where series.nCdApi == IDApi
                                               select series).First();
                                if (urlImagem != serie.sDsImgFanart)
                                {
                                    serie.sDsImgFanart = urlImagem;
                                    db.SaveChanges();
                                    await Helper.DownloadImagesAsync(serie, Enums.TipoImagem.Fanart);
                                }
                            }
                        }
                        else
                        {
                            using (var db = new Context())
                            {
                                Serie serie = (from series in db.Serie
                                               where series.nCdApi == IDApi
                                               select series).First();
                                if (urlImagem != serie.sDsImgPoster)
                                {
                                    serie.sDsImgPoster = urlImagem;
                                    db.SaveChanges();
                                    await Helper.DownloadImagesAsync(serie, Enums.TipoImagem.Poster);
                                }
                            }
                        }
                    }
                }
            }

            Settings.Default.API_UltimaDataAtualizacaoTVDB = dataAtualizacao;
            Settings.Default.Save();
            return true;
        }

        public static async Task<Serie> GetSerieInfoAsync(int IDTvdb, string lang)
        {
            string xmlString = null;
            var rnd = new Random();
            int randomNum = rnd.Next(1, 55555);
            // Para evitar erros de arquivos sendo utilizados ao excluir.
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                       Settings.Default.AppName, "Metadata", "temp" + randomNum, "temp.zip");
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                using (var wc = new WebClient())
                {
                    await
                        wc.DownloadFileTaskAsync(
                                                 new Uri(Settings.Default.API_UrlTheTVDB + "/api/" +
                                                         Settings.Default.API_KeyTheTVDB +
                                                         "/series/" + IDTvdb + "/all/" + lang + ".zip"), path);
                }

                using (ZipFile zip = ZipFile.Read(path))
                {
                    ZipEntry xmlFileEntry = zip[lang + ".xml"];
                    using (var ms = new MemoryStream())
                    {
                        xmlFileEntry.Extract(ms);
                        var sr = new StreamReader(ms);
                        ms.Position = 0;
                        xmlString = sr.ReadToEnd();
                    }
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                if (Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.Delete(Path.GetDirectoryName(path));
                }
            }

            var data = new SeriesData();
            var serializer = new XmlSerializer(typeof(SeriesData));

            using (var reader = new StringReader(xmlString))
            {
                data = (SeriesData) serializer.Deserialize(reader);
            }

            foreach (Serie item in data.Series)
            {
                item.nIdEstado = Enums.Estado.Completo;
                item.lstEpisodios = data.Episodios != null
                                        ? new List<Episodio>(data.Episodios)
                                        : new List<Episodio>();
            }

            return data.Series.FirstOrDefault();
        }

        public static async Task<List<Serie>> GetSeriesAsync(string query)
        {
            string responseData = null;

            try
            {
                using (var httpClient = new HttpClient {BaseAddress = new Uri(Settings.Default.API_UrlTheTVDB)})
                {
                    using (
                        HttpResponseMessage response =
                            await httpClient.GetAsync("/api/GetSeries.php?seriesname=" + query))
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch
            {
                return new List<Serie>();
            }

            // Valida quando o xml possui a tag <Language> em com o 'L' minúsculo.
            responseData = Regex.Replace(responseData, @"(?:<language>)([\w\W]{0,2})(?:<\/language>)",
                                         "<Language>$1</Language>");

            var data = new SeriesData();
            var serializer = new XmlSerializer(typeof(SeriesData));

            using (var reader = new StringReader(responseData))
            {
                data = (SeriesData) serializer.Deserialize(reader);
            }

            var series = new List<Serie>();
            if (data.Series != null)
            {
                foreach (Serie item in data.Series)
                {
                    bool isExistente = series.Select(x => x.nCdApi).Contains(item.nCdApi);
                    if (!isExistente)
                    {
                        item.nIdEstado = Enums.Estado.Simples;
                        series.Add(item);
                    }
                }
            }

            return series;
        }

        public static List<Serie> GetSeries(string query)
        {
            string responseData = null;

            using (var httpClient = new HttpClient {BaseAddress = new Uri(Settings.Default.API_UrlTheTVDB)})
            {
                using (
                    HttpResponseMessage response = httpClient.GetAsync("/api/GetSeries.php?seriesname=" + query).Result)
                {
                    responseData = response.Content.ReadAsStringAsync().Result;
                }
            }

            // Valida quando o xml possui a tag <Language> em com o 'L' minúsculo.
            responseData = Regex.Replace(responseData, @"(?:<language>)([\w\W]{0,2})(?:<\/language>)",
                                         "<Language>$1</Language>");

            var data = new SeriesData();
            var serializer = new XmlSerializer(typeof(SeriesData));

            using (var reader = new StringReader(responseData))
            {
                data = (SeriesData) serializer.Deserialize(reader);
            }

            var series = new List<Serie>();
            if (data.Series != null)
            {
                foreach (Serie itemData in data.Series)
                {
                    foreach (Serie item in data.Series)
                    {
                        var isExistente = false;
                        item.nIdEstado = Enums.Estado.Simples;
                        foreach (Serie itemListaSeries in series)
                        {
                            if (item.nCdApi == itemListaSeries.nCdApi)
                            {
                                isExistente = true;
                                break;
                            }
                        }

                        if (!isExistente)
                        {
                            series.Add(item);
                        }
                    }
                    //foreach (var item in series)
                    //{
                    //    if (item.IDApi == itemData.IDApi)
                    //    {
                    //        isExistente = true;
                    //        break;
                    //    }
                    //}
                    //if (!isExistente)
                    //    series.Add(itemData);
                    //break;
                }
            }

            return series;
        }

        private static async Task<Episodio> GetEpisodeInfoAsync(int IDApi, string pref_IdiomaPesquisa)
        {
            string responseData = null;

            using (var httpClient = new HttpClient {BaseAddress = new Uri(Settings.Default.API_UrlTheTVDB)})
            {
                using (
                    HttpResponseMessage response =
                        await
                        httpClient.GetAsync("/api/" + Settings.Default.API_KeyTheTVDB + "/episodes/" + IDApi + "/" +
                                            Settings.Default.pref_IdiomaPesquisa + ".xml"))
                {
                    responseData = await response.Content.ReadAsStringAsync();
                }
            }

            var episode = new Episodio();
            var serializer = new XmlSerializer(typeof(SeriesData));

            using (var reader = new StringReader(responseData))
            {
                var data = (SeriesData) serializer.Deserialize(reader);
                episode = data.Episodios[0];
            }
            return episode;
        }
    }
}
