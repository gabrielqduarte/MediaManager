﻿// Developed by: Gabriel Duarte
// 
// Created at: 01/11/2015 00:02

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MediaManager.Commands;
using MediaManager.Helpers;
using MediaManager.Model;

namespace MediaManager.ViewModel
{
    public class EpisodiosViewModel : ViewModelBase
    {
        private Array _ArrayEstadoEpisodio;

        private bool? _bFlSelecionarTodos;

        private ObservableCollection<Episodio> _lstEpisodios;

        private ICollectionView _lstEpisodiosView;

        private Enums.EstadoEpisodio _nIdEstadoEpisodioSelecionado;

        public EpisodiosViewModel(List<Episodio> episodios)
        {
            lstEpisodios = new ObservableCollection<Episodio>(episodios);
            lstEpisodiosView = new ListCollectionView(lstEpisodios);
            lstEpisodiosView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Episodio.nNrTemporada)));
            bFlSelecionarTodos = false;
            var lstEstadosParaExibir = new List<int> {0, 1, 2, 4, 5};
            ArrayEstadoEpisodio = Enum.GetValues(typeof(Enums.EstadoEpisodio))
                                      .Cast<Enums.EstadoEpisodio>()
                                      .Where(x => lstEstadosParaExibir.Contains((int) x))
                                      .ToArray();
            nIdEstadoEpisodioSelecionado = Enums.EstadoEpisodio.Selecione;
            CommandSelecionarTodos = new EpisodiosCommand.CommandSelecionarTodos();
            CommandIsSelected = new EpisodiosCommand.CommandIsSelected();
            CommandSalvar = new EpisodiosCommand.CommandSalvar();
            CommandFechar = new EpisodiosCommand.CommandFechar();
        }

        public ICollectionView lstEpisodiosView
        {
            get { return _lstEpisodiosView; }
            set
            {
                _lstEpisodiosView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Episodio> lstEpisodios
        {
            get { return _lstEpisodios; }
            set
            {
                _lstEpisodios = value;
                OnPropertyChanged();
            }
        }

        public bool? bFlSelecionarTodos
        {
            get { return _bFlSelecionarTodos; }
            set
            {
                _bFlSelecionarTodos = value;
                OnPropertyChanged();
            }
        }

        public Array ArrayEstadoEpisodio
        {
            get { return _ArrayEstadoEpisodio; }
            set
            {
                _ArrayEstadoEpisodio = value;
                OnPropertyChanged();
            }
        }

        public Enums.EstadoEpisodio nIdEstadoEpisodioSelecionado
        {
            get { return _nIdEstadoEpisodioSelecionado; }
            set
            {
                _nIdEstadoEpisodioSelecionado = value;
                OnPropertyChanged();
            }
        }

        public Action ActionFechar { get; set; }

        public ICommand CommandSelecionarTodos { get; set; }

        public ICommand CommandIsSelected { get; set; }

        public ICommand CommandSalvar { get; set; }

        public ICommand CommandFechar { get; set; }
    }
}
