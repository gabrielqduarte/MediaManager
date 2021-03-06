﻿// Developed by: Gabriel Duarte
// 
// Created at: 20/09/2015 17:53

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using MediaManager.Commands;
using MediaManager.Helpers;
using MediaManager.Model;

namespace MediaManager.ViewModel
{
    public class RenomearViewModel : ViewModelBase
    {
        private bool? _bFlSelecionarTodos;

        private ObservableCollection<Episodio> _lstEpisodios;

        public RenomearViewModel(bool bFlConsiderarArquivosJaRenomeados, IEnumerable<FileInfo> arquivos = null)
        {
            CommandRenomear = new RenomearCommands.CommandRenomear();
            CommandSelecionar = new RenomearCommands.CommandSelecionar();
            CommandSelecionarTodos = new RenomearCommands.CommandSelecionarTodos();

            //lstEpisodiosView.SortDescriptions.Add(new SortDescription("oSerie.sDsTitulo", ListSortDirection.Ascending));
            //lstEpisodiosView.IsLiveSortingRequested = true;

            if (arquivos != null)
            {
                arquivos = new DirectoryInfo(Properties.Settings.Default.pref_PastaDownloads).EnumerateFiles("*.*",
                                                                                                             SearchOption
                                                                                                                 .AllDirectories);
            }
            Carregar(arquivos, bFlConsiderarArquivosJaRenomeados);
            CommandSelecionar.Execute(this);
        }

        public bool bFlSilencioso { get; set; }

        public bool? bFlSelecionarTodos
        {
            get { return _bFlSelecionarTodos; }
            set
            {
                _bFlSelecionarTodos = value;
                OnPropertyChanged();
            }
        }

        public Action ActionFechar { get; set; } // Para poder fechar depois no RenomearCommand

        public ObservableCollection<Episodio> lstEpisodios
        {
            get { return _lstEpisodios; }
            set
            {
                _lstEpisodios = value;
                OnPropertyChanged();
            }
        }

        public ICommand CommandRenomear { get; set; }

        public ICommand CommandSelecionar { get; set; }

        public ICommand CommandSelecionarTodos { get; set; }

        private void Carregar(IEnumerable<FileInfo> arquivos, bool bFlConsiderarArquivosJaRenomeados)
        {
            this.lstEpisodios = new ObservableCollection<Episodio>();
            this.lstEpisodios.Add(new Episodio
            {
                oSerie = new Serie {sDsTitulo = "Carregando..."},
                sDsFilepathOriginal = "Carregando...",
                bFlSelecionado = false
            });

            string[] extensoesPermitidas = Properties.Settings.Default.ExtensoesRenomeioPermitidas.Split('|');
            var lstEpisodios = new List<Episodio>();
            var lstEpisodiosNaoEncontrados = new List<Episodio>();
            if (arquivos != null)
            {
                foreach (FileInfo item in arquivos)
                {
                    if (extensoesPermitidas.Contains(item.Extension))
                    {
                        var episodio = new Episodio();
                        episodio.sDsFilepath = item.FullName;
                        if ((!bFlConsiderarArquivosJaRenomeados && episodio.IdentificarEpisodio() &&
                             !episodio.bFlRenomeado && episodio.nIdEstadoEpisodio != Enums.EstadoEpisodio.Arquivado)
                            || (bFlConsiderarArquivosJaRenomeados && episodio.IdentificarEpisodio()))
                        {
                            episodio.sDsFilepathOriginal = item.FullName;
                            episodio.sDsFilepath = Path.Combine(episodio.oSerie.sDsPasta,
                                                                Helper.RenomearConformePreferencias(episodio) +
                                                                item.Extension);
                            //episodio.bFlRenomeado = true; // Habilitando a linha da problema no retorno dos arquivos renomeados via argumento (RenomearEpisodiosDosArgumentos())
                            lstEpisodios.Add(episodio);
                        }
                        else
                        {
                            lstEpisodiosNaoEncontrados.Add(episodio);
                        }
                    }
                }
            }
            //foreach (var item in listaEpisodiosNotFound)
            //{
            //    TextWriter tw = new StreamWriter(@"C:/Episodios não encontrados.txt", true);
            //    tw.WriteLine(item.Filename);
            //    tw.Close();
            //}
            this.lstEpisodios = new ObservableCollection<Episodio>(lstEpisodios);
        }
    }
}
