﻿// Developed by: Gabriel Duarte
// 
// Created at: 22/11/2015 18:08

using System;
using System.Linq;
using System.Windows.Input;
using Autofac;
using MediaManager.Forms;
using MediaManager.Helpers;
using MediaManager.Model;
using MediaManager.Services;
using MediaManager.ViewModel;

namespace MediaManager.Commands
{
    public class ProcurarConteudoCommands
    {
        public class CommandAdicionar : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return parameter is ProcurarConteudoViewModel
                       && (parameter as ProcurarConteudoViewModel).lstConteudos.Count > 0
                       &&
                       (parameter as ProcurarConteudoViewModel).lstConteudos.Where(
                                                                                   x =>
                                                                                   x.bFlSelecionado &&
                                                                                   !x.bFlNaoEncontrado).Count() > 0;
            }

            public void Execute(object parameter)
            {
                var ProcurarConteudoViewModel = parameter as ProcurarConteudoViewModel;
                var frmBarraProgresso = new frmBarraProgresso();
                frmBarraProgresso.BarraProgressoViewModel.dNrProgressoMaximo =
                    ProcurarConteudoViewModel.lstConteudos.Where(x => x.bFlSelecionado).Count();
                frmBarraProgresso.BarraProgressoViewModel.sDsTarefa = "Salvando...";
                frmBarraProgresso.BarraProgressoViewModel.Worker.DoWork += (s, ev) =>
                {
                    var seriesService = App.Container.Resolve<SeriesService>();
                    //if (ProcurarConteudoViewModel.lstConteudos.Where(x => x.bFlSelecionado && !x.bFlNaoEncontrado).Count() == 0)
                    //{
                    //    Helper.MostrarMensagem("Para realizar a operação, selecione ao menos um registro.", Enums.eTipoMensagem.Alerta);
                    //}
                    foreach (Video item in ProcurarConteudoViewModel.lstConteudos)
                    {
                        if (item.bFlSelecionado && !item.bFlNaoEncontrado)
                        {
                            switch (item.nIdTipoConteudo)
                            {
                                case Enums.TipoConteudo.Série:
                                case Enums.TipoConteudo.Anime:
                                {
                                    frmBarraProgresso.BarraProgressoViewModel.sDsTexto = "Salvando \"" + item.sDsTitulo +
                                                                                         "\" (" +
                                                                                         (frmBarraProgresso
                                                                                              .BarraProgressoViewModel
                                                                                              .dNrProgressoAtual + 1) +
                                                                                         "/" +
                                                                                         frmBarraProgresso
                                                                                             .BarraProgressoViewModel
                                                                                             .dNrProgressoMaximo +
                                                                                         ")...";

                                    if (item.nIdEstado != Enums.Estado.Completo)
                                    {
                                        Serie serie =
                                            APIRequests.GetSerieInfoAsync(item.nCdApi,
                                                                          Properties.Settings.Default
                                                                                    .pref_IdiomaPesquisa).Result;
                                        serie.nIdTipoConteudo = item.nIdTipoConteudo;
                                        serie.sDsPasta = item.sDsPasta;
                                        serie.sAliases = item.sAliases;
                                        serie.lstSerieAlias = item.lstSerieAlias;
                                        serie.sDsTitulo = item.sDsTitulo;
                                        serie.SetEstadoEpisodio();
                                        seriesService.Adicionar(serie);
                                    }
                                    else
                                    {
                                        (item as Serie).SetEstadoEpisodio();
                                        seriesService.Adicionar((Serie) item);
                                    }

                                    frmBarraProgresso.BarraProgressoViewModel.dNrProgressoAtual++;

                                    if (frmBarraProgresso.BarraProgressoViewModel.dNrProgressoAtual ==
                                        frmBarraProgresso.BarraProgressoViewModel.dNrProgressoMaximo)
                                    {
                                        frmBarraProgresso.BarraProgressoViewModel.sDsTexto = "Concluído.";
                                    }
                                    break;
                                }
                                case Enums.TipoConteudo.Filme:
                                    throw new NotImplementedException(); // TODO Filmes
                            }
                        }
                    }

                    Helper.MostrarMensagem("Séries inseridas com sucesso.", Enums.eTipoMensagem.Informativa);
                };

                frmBarraProgresso.BarraProgressoViewModel.Worker.RunWorkerCompleted += (s, ev) => ProcurarConteudoViewModel.ActionFechar();
                frmBarraProgresso.BarraProgressoViewModel.Worker.RunWorkerAsync();
                frmBarraProgresso.ShowDialog(ProcurarConteudoViewModel.Owner);
            }
        }

        public class CommandSelecionar : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return parameter is ProcurarConteudoViewModel;
            }

            public void Execute(object parameter)
            {
                var procurarConteudoVM = parameter as ProcurarConteudoViewModel;
                int conteudosSelecionadosCount = procurarConteudoVM.lstConteudos.Where(x => x.bFlSelecionado).Count();
                if (conteudosSelecionadosCount == procurarConteudoVM.lstConteudos.Count &&
                    procurarConteudoVM.lstConteudos.Count > 0)
                {
                    procurarConteudoVM.bFlSelecionarTodos = true;
                }
                else if (conteudosSelecionadosCount == 0)
                {
                    procurarConteudoVM.bFlSelecionarTodos = false;
                }
                else if (conteudosSelecionadosCount > 0)
                {
                    procurarConteudoVM.bFlSelecionarTodos = null;
                }
            }
        }

        public class CommandSelecionarTodos : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                return parameter is ProcurarConteudoViewModel;
            }

            public void Execute(object parameter)
            {
                var procurarConteudoVM = parameter as ProcurarConteudoViewModel;
                if (procurarConteudoVM.bFlSelecionarTodos == true)
                {
                    procurarConteudoVM.lstConteudos.ToList().ForEach(x => x.bFlSelecionado = true);
                }
                else
                {
                    procurarConteudoVM.bFlSelecionarTodos = false;
                    procurarConteudoVM.lstConteudos.ToList().ForEach(x => x.bFlSelecionado = false);
                }
            }
        }
    }
}
