﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Corvus.Crypto;
using Corvus.DarkOrbit;
using Corvus.DarkOrbit.Data;
using Corvus.EnumExtension;
using Corvus.Http;
using Corvus.Time;
using IniParser;
using IniParser.Model;

namespace Corvus
{
    public partial class FrmMain : Form
    {
        private DarkOrbitAccount _account;


        #region Skylab DGV
        private DataGridViewRow _prometiumCollectorRow;
        private DataGridViewRow _enduriumCollectorRow;
        private DataGridViewRow _terbiumCollectorRow;
        private DataGridViewRow _solarModuleRow;
        private DataGridViewRow _storageModuleRow;
        private DataGridViewRow _baseModuleRow;
        private DataGridViewRow _prometidRefineryRow;
        private DataGridViewRow _duraniumRefineryRow;
        private DataGridViewRow _promeriumRefineryRow;
        private DataGridViewRow _xenoModuleRow;
        private DataGridViewRow _sepromRefineryRow;

        private int _dgvSkylabLevel = 1;
        private int _dgvSkylabUpgrading = 2;
        private int _dgvSkylabTimeLeft = 3;
        private int _dgvSkylabUpgrade = 4;
        #endregion

        #region TechFactory DGV
        private DataGridViewRow _techHall1Row;
        private DataGridViewRow _techHall2Row;
        private DataGridViewRow _techHall3Row;

        private int _dgvTechFactoryName = 0;
        private int _dgvTechFactoryAmount = 1;
        private int _dgvTechFactoryBuilding = 2;
        private int _dgvTechFactoryTimeLeft = 3;
        private int _dgvTechFactoryBuild = 4;
        #endregion

        #region Gate DGV
        private DataGridViewRow _alphaRow;
        private DataGridViewRow _betaRow;
        private DataGridViewRow _gammaRow;
        private DataGridViewRow _deltaRow;
        private DataGridViewRow _epsilonRow;
        private DataGridViewRow _zetaRow;
        private DataGridViewRow _kappaRow;
        private DataGridViewRow _lambdaRow;
        private DataGridViewRow _hadesRow;
        private DataGridViewRow _kuiperRow;

        private int _dgvGateParts = 1;
        private int _dgvGateReady = 2;
        private int _dgvGateOnMap = 3;
        private int _dgvGateGatesBuilt = 4;
        #endregion

        #region Gate GatesBuilt
        private int GatesBuiltAlpha = 0;
        private int GatesBuiltBeta = 0;
        private int GatesBuiltGamma = 0;
        private int GatesBuiltDelta = 0;
        private int GatesBuiltEpsilon = 0;
        private int GatesBuiltZeta = 0;
        private int GatesBuiltKappa = 0;
        private int GatesBuiltLambda = 0;
        private int GatesBuiltHades = 0;
        private int GatesBuiltKuiper = 0;
        #endregion

        private Task _runTask;
        private bool _running = false;
        CancellationTokenSource _cancellationTokenSource = null;
        private bool manuallyStopped = false;
        private bool loginAccount = false;
        private bool useMultiplier = false;

        private DateTime _nextRunTechFactory = DateTime.Now;
        private DateTime _nextRunSkylab = DateTime.Now;
        private DateTime _nextRunGalaxyGate = DateTime.Now;
        private DateTime _nextRefreshGalaxyGate = DateTime.Now;

        public FrmMain()
        {
            InitializeComponent();
            DisableGui();
            tabPageLogin.Enabled = true;
            comboBoxLoginPortal.SelectedIndex = 0;
            comboBoxOptionABG.SelectedIndex = 0;
            comboBoxMultiplier.SelectedIndex = 0;
            comboBoxEnergyBoost.SelectedIndex = 0;

            if (chkBoxActLog.Checked)
            {
                tcMain.TabPages.Insert(4, tabPageLog);
            }
            else
            {
                tcMain.TabPages.Remove(tabPageLog);
            }

            if (chkBoxSmartMultiplier.Checked)
            {
                lblMultiplier.Enabled = false;
                comboBoxMultiplier.Enabled = false;
            }
            else
            {
                lblMultiplier.Enabled = true;
                comboBoxMultiplier.Enabled = true;
            }

            _prometiumCollectorRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Prometium Collector", 0, false, "", false)];
            _enduriumCollectorRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Endurium Collector", 0, false, "", false)];
            _terbiumCollectorRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Terbium Collector", 0, false, "", false)];
            _solarModuleRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Solar Module", 0, false, "", false)];
            _storageModuleRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Storage Module", 0, false, "", false)];
            _baseModuleRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Base Module", 0, false, "", false)];
            _prometidRefineryRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Prometid Refinery", 0, false, "", false)];
            _duraniumRefineryRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Duranium Refinery", 0, false, "", false)];
            _promeriumRefineryRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Promerium Refinery", 0, false, "", false)];
            _xenoModuleRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Xeno Module", 0, false, "", false)];
            _sepromRefineryRow = dgvSkylab.Rows[dgvSkylab.Rows.Add("Seprom Refinery", 0, false, "", false)];

            _techHall1Row = dgvTechFactory.Rows[dgvTechFactory.Rows.Add("", 0, false, "", false)];
            _techHall1Row.Visible = false;
            _techHall2Row = dgvTechFactory.Rows[dgvTechFactory.Rows.Add("", 0, false, "", false)];
            _techHall2Row.Visible = false;
            _techHall3Row = dgvTechFactory.Rows[dgvTechFactory.Rows.Add("", 0, false, "", false)];
            _techHall3Row.Visible = false;

            _alphaRow = dgvGates.Rows[dgvGates.Rows.Add("Alpha", 0, false, false, 0)];
            _betaRow = dgvGates.Rows[dgvGates.Rows.Add("Beta", 0, false, false, 0)];
            _gammaRow = dgvGates.Rows[dgvGates.Rows.Add("Gamma", 0, false, false, 0)];
            _deltaRow = dgvGates.Rows[dgvGates.Rows.Add("Delta", 0, false, false, 0)];
            _epsilonRow = dgvGates.Rows[dgvGates.Rows.Add("Epsilon", 0, false, false, 0)];
            _zetaRow = dgvGates.Rows[dgvGates.Rows.Add("Zeta", 0, false, false, 0)];
            _kappaRow = dgvGates.Rows[dgvGates.Rows.Add("Kappa", 0, false, false, 0)];
            _lambdaRow = dgvGates.Rows[dgvGates.Rows.Add("Lambda", 0, false, false, 0)];
            _hadesRow = dgvGates.Rows[dgvGates.Rows.Add("Hades", 0, false, false, 0)];
            _kuiperRow = dgvGates.Rows[dgvGates.Rows.Add("Kuiper", 0, false, false, 0)];

            Log($"Corvus v{Assembly.GetExecutingAssembly().GetName().Version} started - Made by 'Heaven. and SrFairyox");

            LoadSettings();
        }

        private GalaxyGate GetSelectedGate()
        {
            if (rbBuildABG.Checked)
            {
                return GalaxyGate.Alpha;
            }

            if (rbBuildDelta.Checked)
            {
                return GalaxyGate.Delta;
            }

            if (rbBuildEpsilon.Checked)
            {
                return GalaxyGate.Epsilon;
            }

            if (rbBuildZeta.Checked)
            {
                return GalaxyGate.Zeta;
            }

            if (rbBuildKappa.Checked)
            {
                return GalaxyGate.Kappa;
            }

            if (rbBuildLambda.Checked)
            {
                return GalaxyGate.Lambda;
            }

            if (rbBuildHades.Checked)
            {
                return GalaxyGate.Hades;
            }

            if (rbBuildKuiper.Checked)
            {
                return GalaxyGate.Kuiper;
            }

            return GalaxyGate.None;
        }

        private void DisableGui()
        {
            tabPageLogin.Enabled = false;
            tabPageGalaxyGates.Enabled = false;
            tabPageTechFactory.Enabled = false;
            tabPageSkylab.Enabled = false;
            cmdStart.Enabled = false;
            cmdStop.Enabled = false;
            cmdOpenBackPage.Enabled = false;
            cmdSaveSettings.Enabled = false;
        }

        private void EnableGui()
        {
            tabPageLogin.Enabled = true;
            tabPageGalaxyGates.Enabled = true;
            tabPageTechFactory.Enabled = true;
            tabPageSkylab.Enabled = true;
            cmdStart.Enabled = true;
            cmdOpenBackPage.Enabled = true;
            cmdSaveSettings.Enabled = true;
        }

        private void UpdateSkylabGui()
        {
            Log("Updating Skylab gui...");
            _prometiumCollectorRow.Cells[_dgvSkylabLevel].Value =_account.SkylabData.PrometiumCollectorInfo.Level;
            _prometiumCollectorRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.PrometiumCollectorInfo.Upgrading;
            _prometiumCollectorRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.PrometiumCollectorInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.PrometiumCollectorInfo.Level == 20)
            {
                _prometiumCollectorRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _enduriumCollectorRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.EnduriumCollectorInfo.Level;
            _enduriumCollectorRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.EnduriumCollectorInfo.Upgrading;
            _enduriumCollectorRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.EnduriumCollectorInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.EnduriumCollectorInfo.Level == 20)
            {
                _enduriumCollectorRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _terbiumCollectorRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.TerbiumCollectorInfo.Level;
            _terbiumCollectorRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.TerbiumCollectorInfo.Upgrading;
            _terbiumCollectorRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.TerbiumCollectorInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.TerbiumCollectorInfo.Level == 20)
            {
                _terbiumCollectorRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _solarModuleRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.SolarModuleInfo.Level;
            _solarModuleRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.SolarModuleInfo.Upgrading;
            _solarModuleRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.SolarModuleInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.SolarModuleInfo.Level == 20)
            {
                _solarModuleRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _storageModuleRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.StorageModuleInfo.Level;
            _storageModuleRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.StorageModuleInfo.Upgrading;
            _storageModuleRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.StorageModuleInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.StorageModuleInfo.Level == 20)
            {
                _storageModuleRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _baseModuleRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.BaseModuleInfo.Level;
            _baseModuleRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.BaseModuleInfo.Upgrading;
            _baseModuleRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.BaseModuleInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.BaseModuleInfo.Level == 20)
            {
                _baseModuleRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _prometidRefineryRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.PrometidRefineryInfo.Level;
            _prometidRefineryRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.PrometidRefineryInfo.Upgrading;
            _prometidRefineryRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.PrometidRefineryInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.PrometidRefineryInfo.Level == 20)
            {
                _prometidRefineryRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _duraniumRefineryRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.DuraniumRefineryInfo.Level;
            _duraniumRefineryRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.DuraniumRefineryInfo.Upgrading;
            _duraniumRefineryRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.DuraniumRefineryInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.DuraniumRefineryInfo.Level == 20)
            {
                _duraniumRefineryRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _promeriumRefineryRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.PromeriumRefineryInfo.Level;
            _promeriumRefineryRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.PromeriumRefineryInfo.Upgrading;
            _promeriumRefineryRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.PromeriumRefineryInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.PromeriumRefineryInfo.Level == 20)
            {
                _promeriumRefineryRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _xenoModuleRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.XenoModuleInfo.Level;
            _xenoModuleRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.XenoModuleInfo.Upgrading;
            _xenoModuleRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.XenoModuleInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.XenoModuleInfo.Level == 20)
            {
                _xenoModuleRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }

            _sepromRefineryRow.Cells[_dgvSkylabLevel].Value = _account.SkylabData.SepromRefineryInfo.Level;
            _sepromRefineryRow.Cells[_dgvSkylabUpgrading].Value = _account.SkylabData.SepromRefineryInfo.Upgrading;
            _sepromRefineryRow.Cells[_dgvSkylabTimeLeft].Value = _account.SkylabData.SepromRefineryInfo.TimeLeft.FormatReadable();
            if (_account.SkylabData.SepromRefineryInfo.Level == 20)
            {
                _sepromRefineryRow.Cells[_dgvSkylabUpgrade].ReadOnly = true;
            }
        }

        private async Task ExecuteSkylabAsync()
        {
            Log("Reading Skylab...");
            await _account.ReadSkylabAsync();
            UpdateSkylabGui();

            if ((bool) (_prometiumCollectorRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.PrometiumCollectorInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Prometium Collector...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.PrometiumCollectorName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_enduriumCollectorRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.EnduriumCollectorInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Endurium Collector...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.EnduriumCollectorName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_terbiumCollectorRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.TerbiumCollectorInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Terbium Collector...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.TerbiumCollectorName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_solarModuleRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.SolarModuleInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Solar Module...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.SolarModuleName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_storageModuleRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.StorageModuleInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Storage Module...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.StorageModuleName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_baseModuleRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.BaseModuleInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Base Module...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.BaseModuleName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_prometidRefineryRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.PrometidRefineryInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Prometid Refinery...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.PrometidRefineryName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_duraniumRefineryRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.DuraniumRefineryInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Duranium Refinery...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.DuraniumRefineryName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_promeriumRefineryRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.PromeriumRefineryInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Promerium Refinery...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.PromeriumRefineryName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_xenoModuleRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.XenoModuleInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Xeno Module...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.XenoModuleName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            if ((bool)(_sepromRefineryRow.Cells[_dgvSkylabUpgrade] as DataGridViewCheckBoxCell).Value)
            {
                if (!_account.SkylabData.SepromRefineryInfo.Upgrading)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log("Upgrading Seprom Refinery...");
                        await Task.Delay(5000);
                        if (await _account.UpgradeSkylabAsync(SkylabData.SepromRefineryName))
                        {
                            Log($"Upgrade success!");
                            break;
                        }
                        Log($"Upgrading failed, trying again in 5 seconds...");
                        await Task.Delay(5000);
                    }
                }
            }

            Log("Reading Skylab...");
            await _account.ReadSkylabAsync();
            UpdateSkylabGui();
        }

        private void UpdateTechFactoryGui()
        {
            Log("Updating Tech Factory gui...");
            if (_account.TechFactoryData.Hall1.Enabled)
            {
                _techHall1Row.Visible = true;
                _techHall1Row.Cells[_dgvTechFactoryName].Value = _account.TechFactoryData.Hall1.Item.GetFullName();
                _techHall1Row.Cells[_dgvTechFactoryAmount].Value = _account.TechFactoryData.Hall1.Amount;
                _techHall1Row.Cells[_dgvTechFactoryBuilding].Value = _account.TechFactoryData.Hall1.Building;
                _techHall1Row.Cells[_dgvTechFactoryTimeLeft].Value = _account.TechFactoryData.Hall1.TimeLeft.FormatReadable();
                if (string.IsNullOrEmpty(_account.TechFactoryData.Hall1.Item.GetFullName()))
                {
                    _techHall1Row.Cells[_dgvTechFactoryName].Value = "Please build a tech!";
                    _techHall1Row.Cells[_dgvTechFactoryBuild].ReadOnly = true;
                }
            }
            if (_account.TechFactoryData.Hall2.Enabled)
            {
                _techHall2Row.Visible = true;
                _techHall2Row.Cells[_dgvTechFactoryName].Value = _account.TechFactoryData.Hall2.Item.GetFullName();
                _techHall2Row.Cells[_dgvTechFactoryAmount].Value = _account.TechFactoryData.Hall2.Amount;
                _techHall2Row.Cells[_dgvTechFactoryBuilding].Value = _account.TechFactoryData.Hall2.Building;
                _techHall2Row.Cells[_dgvTechFactoryTimeLeft].Value = _account.TechFactoryData.Hall2.TimeLeft.FormatReadable();
                if (string.IsNullOrEmpty(_account.TechFactoryData.Hall2.Item.GetFullName()))
                {
                    _techHall2Row.Cells[_dgvTechFactoryName].Value = "Please build a tech!";
                    _techHall2Row.Cells[_dgvTechFactoryBuild].ReadOnly = true;
                }
            }
            if (_account.TechFactoryData.Hall3.Enabled)
            {
                _techHall3Row.Visible = true;
                _techHall3Row.Cells[_dgvTechFactoryName].Value = _account.TechFactoryData.Hall3.Item.GetFullName();
                _techHall3Row.Cells[_dgvTechFactoryAmount].Value = _account.TechFactoryData.Hall3.Amount;
                _techHall3Row.Cells[_dgvTechFactoryBuilding].Value = _account.TechFactoryData.Hall3.Building;
                _techHall3Row.Cells[_dgvTechFactoryTimeLeft].Value = _account.TechFactoryData.Hall3.TimeLeft.FormatReadable();
                if (string.IsNullOrEmpty(_account.TechFactoryData.Hall3.Item.GetFullName()))
                {
                    _techHall3Row.Cells[_dgvTechFactoryName].Value = "Please build a tech!";
                    _techHall3Row.Cells[_dgvTechFactoryBuild].ReadOnly = true;
                }
            }
        }

        private async Task ExecuteTechFactoryAsync()
        {
            Log("Reading Tech Factory...");
            await _account.ReadTechFactoryAsync();
            UpdateTechFactoryGui();

            if (_account.TechFactoryData.Hall1.Enabled && _account.TechFactoryData.Hall1.Item != TechFactoryData.TechFactoryItem.None)
            {
                if (!_account.TechFactoryData.Hall1.Building && (bool)(_techHall1Row.Cells[_dgvTechFactoryBuild] as DataGridViewCheckBoxCell).Value)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log($"Building {_account.TechFactoryData.Hall1.Item.GetFullName()} on hall 1...");
                        if (await _account.BuildTechAsync(_account.TechFactoryData.Hall1.Item, 1))
                        {
                            Log($"Building success!");
                            break;
                        }
                        else
                        {
                            Log($"Building failed, trying again in 5 seconds...");
                            await Task.Delay(5000);
                        }
                    }
                }
            }

            if (_account.TechFactoryData.Hall2.Enabled && _account.TechFactoryData.Hall2.Item != TechFactoryData.TechFactoryItem.None)
            {
                if (!_account.TechFactoryData.Hall2.Building && (bool)(_techHall2Row.Cells[_dgvTechFactoryBuild] as DataGridViewCheckBoxCell).Value)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log($"Building {_account.TechFactoryData.Hall2.Item.GetFullName()} on hall 2...");
                        if (await _account.BuildTechAsync(_account.TechFactoryData.Hall2.Item, 2))
                        {
                            Log($"Building success!");
                            break;
                        }
                        else
                        {
                            Log($"Building failed, trying again in 5 seconds...");
                            await Task.Delay(5000);
                        }
                    }
                }
            }


            if (_account.TechFactoryData.Hall3.Enabled && _account.TechFactoryData.Hall3.Item != TechFactoryData.TechFactoryItem.None)
            {
                if (!_account.TechFactoryData.Hall3.Building && (bool)(_techHall3Row.Cells[_dgvTechFactoryBuild] as DataGridViewCheckBoxCell).Value)
                {
                    for (var i = 0; i < 2; i++)
                    {
                        Log($"Building {_account.TechFactoryData.Hall3.Item.GetFullName()} on hall 3...");
                        if (await _account.BuildTechAsync(_account.TechFactoryData.Hall3.Item, 3))
                        {
                            Log($"Building success!");
                            break;
                        }
                        else
                        {
                            Log($"Building failed, trying again in 5 seconds...");
                            await Task.Delay(5000);
                        }
                    }
                }
            }

            Log("Reading Tech Factory...");
            await _account.ReadTechFactoryAsync();
            UpdateTechFactoryGui();
        }

        private void UpdateGateGui()
        {
            Log("Updating Gate gui...");
            Invoke(new Action(() =>
            {
                lblSpinCost.Text = $"Spin cost: {_account.GateData.EnergyCost.Text}";
                lblUridium.Text = $"Uridium: {_account.GateData.Money}";
                lblExtraEnergy.Text = $"Extra Energy: {_account.GateData.Samples}";

                _alphaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Alpha().Current}/{_account.GateData.Gates.Alpha().Total}";
                _alphaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Alpha().Current == _account.GateData.Gates.Alpha().Total;
                _alphaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Alpha().Prepared;
                _alphaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltAlpha;

                _betaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Beta().Current}/{_account.GateData.Gates.Beta().Total}";
                _betaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Beta().Current == _account.GateData.Gates.Beta().Total;
                _betaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Beta().Prepared;
                _betaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltBeta;

                _gammaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Gamma().Current}/{_account.GateData.Gates.Gamma().Total}";
                _gammaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Gamma().Current == _account.GateData.Gates.Gamma().Total;
                _gammaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Gamma().Prepared;
                _gammaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltGamma;

                _deltaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Delta().Current}/{_account.GateData.Gates.Delta().Total}";
                _deltaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Delta().Current == _account.GateData.Gates.Delta().Total;
                _deltaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Delta().Prepared;
                _deltaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltDelta;

                _epsilonRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Epsilon().Current}/{_account.GateData.Gates.Epsilon().Total}";
                _epsilonRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Epsilon().Current == _account.GateData.Gates.Epsilon().Total;
                _epsilonRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Epsilon().Prepared;
                _epsilonRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltEpsilon;

                _zetaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Zeta().Current}/{_account.GateData.Gates.Zeta().Total}";
                _zetaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Zeta().Current == _account.GateData.Gates.Zeta().Total;
                _zetaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Zeta().Prepared;
                _zetaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltZeta;

                _kappaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Kappa().Current}/{_account.GateData.Gates.Kappa().Total}";
                _kappaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Kappa().Current == _account.GateData.Gates.Kappa().Total;
                _kappaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Kappa().Prepared;
                _kappaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltKappa;

                _lambdaRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Lambda().Current}/{_account.GateData.Gates.Lambda().Total}";
                _lambdaRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Lambda().Current == _account.GateData.Gates.Lambda().Total;
                _lambdaRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Lambda().Prepared;
                _lambdaRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltLambda;

                _hadesRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Hades().Current}/{_account.GateData.Gates.Hades().Total}";
                _hadesRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Hades().Current == _account.GateData.Gates.Hades().Total;
                _hadesRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Hades().Prepared;
                _hadesRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltHades;

                _kuiperRow.Cells[_dgvGateParts].Value = $"{_account.GateData.Gates.Kuiper().Current}/{_account.GateData.Gates.Kuiper().Total}";
                _kuiperRow.Cells[_dgvGateReady].Value = _account.GateData.Gates.Kuiper().Current == _account.GateData.Gates.Kuiper().Total;
                _kuiperRow.Cells[_dgvGateOnMap].Value = _account.GateData.Gates.Kuiper().Prepared;
                _kuiperRow.Cells[_dgvGateGatesBuilt].Value = GatesBuiltKuiper;

                lblTotalSpins.Text = $"Total spins: {_account.GateItemsReceived.TotalSpins}";
                lblReceivedX2.Text = $"X2: {_account.GateItemsReceived.X2}";
                lblReceivedX3.Text = $"X3: {_account.GateItemsReceived.X3}";
                lblReceivedX4.Text = $"X4: {_account.GateItemsReceived.X4}";
                lblReceivedSAB.Text = $"SAB: {_account.GateItemsReceived.SAB}";
                lblReceivedPLT2021.Text = $"PLT-2021: {_account.GateItemsReceived.PLT2021}";
                lblReceivedACM.Text = $"ACM: {_account.GateItemsReceived.ACM}";

                lblReceivedParts.Text = $"Parts: {_account.GateItemsReceived.GateParts}";
                lblReceivedLogDisks.Text = $"Log disks: {_account.GateItemsReceived.LogDisks}";
                lblReceivedRepairCredits.Text = $"Repair credits: {_account.GateItemsReceived.RepairCredits}";
                lblReceivedXenomit.Text = $"Xenomit: {_account.GateItemsReceived.Xenomit}";
                lblReceivedNanoHull.Text = $"Nano hull: {_account.GateItemsReceived.NanoHull}";
            }));
        }

        private async Task InitializeGuiAsync()
        {
            Log("Initializing gui...");
            Log("Reading Skylab...");
            await _account.ReadSkylabAsync();
            UpdateSkylabGui();
            Log("Reading Tech Factory...");
            await _account.ReadTechFactoryAsync();
            UpdateTechFactoryGui();
            Log("Reading Galaxy Gates...");
            await _account.ReadGatesAsync();
            UpdateGateGui();
            Log("Initialization finished!");
        }

        private async Task ExecuteSpinAsync()
        {

            if (DateTime.Now.Subtract(_nextRunGalaxyGate).TotalSeconds <= 0)
                return;

            UpdateGateGui();

            var currentGate = _account.GateData.Gates.Get(GetSelectedGate());

            if  (rbBuildABG.Checked)
            {
                var currentGateA = _account.GateData.Gates.Get(GalaxyGate.Alpha);
                var currentGateB = _account.GateData.Gates.Get(GalaxyGate.Beta);
                var currentGateG = _account.GateData.Gates.Get(GalaxyGate.Gamma);

                if (getOptionforABG() == "option1")
                {
                    if (chkBoxPlaceGate.Checked)
                    {
                        if ((currentGateA.Prepared && currentGateA.Ready) || (currentGateB.Prepared && currentGateB.Ready) || (currentGateG.Prepared && currentGateG.Ready))
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                        if (currentGateA.Ready && !currentGateA.Prepared)
                            await PlaceGateAsync(GalaxyGate.Alpha);
                        if (currentGateB.Ready && !currentGateB.Prepared)
                            await PlaceGateAsync(GalaxyGate.Beta);
                        if (currentGateG.Ready && !currentGateG.Prepared)
                            await PlaceGateAsync(GalaxyGate.Gamma);
                    }
                    else
                    {
                        if (currentGateA.Ready || currentGateB.Ready || currentGateG.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                    }
                    
                }                    
                else if (getOptionforABG() == "option2")
                {
                    if (chkBoxPlaceGate.Checked)
                    {
                        if ((currentGateA.Prepared && currentGateA.Ready) && (currentGateB.Prepared && currentGateB.Ready) && (currentGateG.Prepared && currentGateG.Ready))
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                        if (currentGateA.Ready && !currentGateA.Prepared)
                            await PlaceGateAsync(GalaxyGate.Alpha);
                        if (currentGateB.Ready && !currentGateB.Prepared)
                            await PlaceGateAsync(GalaxyGate.Beta);
                        if (currentGateG.Ready && !currentGateG.Prepared)
                            await PlaceGateAsync(GalaxyGate.Gamma);
                    }
                    else
                    {
                        if (currentGateA.Ready && currentGateB.Ready && currentGateG.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                    }
                }
                    
                else if (getOptionforABG() == "option3")
                {
                    if (chkBoxPlaceGate.Checked)
                    {
                        if (currentGateA.Prepared && currentGateA.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                        if (currentGateA.Ready && !currentGateA.Prepared)
                            await PlaceGateAsync(GalaxyGate.Alpha);
                        if (currentGateB.Ready && !currentGateB.Prepared)
                            await PlaceGateAsync(GalaxyGate.Beta);
                        if (currentGateG.Ready && !currentGateG.Prepared)
                            await PlaceGateAsync(GalaxyGate.Gamma);
                    }
                    else
                    {
                        if (currentGateA.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                    }
                }
                else if (getOptionforABG() == "option4") {
                    if (chkBoxPlaceGate.Checked)
                    {
                        if (currentGateB.Prepared && currentGateB.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                        if (currentGateA.Ready && !currentGateA.Prepared)
                            await PlaceGateAsync(GalaxyGate.Alpha);
                        if (currentGateB.Ready && !currentGateB.Prepared)
                            await PlaceGateAsync(GalaxyGate.Beta);
                        if (currentGateG.Ready && !currentGateG.Prepared)
                            await PlaceGateAsync(GalaxyGate.Gamma);
                    }
                    else
                    {
                        if (currentGateB.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                    }
                }
                    
                else if (getOptionforABG() == "option5")
                {
                    if (chkBoxPlaceGate.Checked)
                    {
                        if (currentGateG.Prepared && currentGateG.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                        if (currentGateA.Ready && !currentGateA.Prepared)
                            await PlaceGateAsync(GalaxyGate.Alpha);
                        if (currentGateB.Ready && !currentGateB.Prepared)
                            await PlaceGateAsync(GalaxyGate.Beta);
                        if (currentGateG.Ready && !currentGateG.Prepared)
                            await PlaceGateAsync(GalaxyGate.Gamma);
                    }
                    else
                    {
                        if (currentGateG.Ready)
                        {
                            Stopping_gate_mode("can_not_get_more_parts");
                            return;
                        }
                    }
                }
                
            } else {
                if (chkBoxPlaceGate.Checked)
                {
                    if (currentGate.Prepared && currentGate.Ready)
                    {
                        Stopping_gate_mode("can_not_get_more_parts");
                        return;
                    }
                    if (currentGate.Ready && !currentGate.Prepared)
                        await PlaceGateAsync(GetSelectedGate());
                }
                else
                {
                    if (currentGate.Ready)
                    {
                        Stopping_gate_mode("can_not_get_more_parts");
                        return;
                    }
                }
            }


            if (_account.GateData.EnergyCost.Text > _account.GateData.Money && _account.GateData.Samples <= 0)
            {
                Stopping_gate_mode("no_uridium/ee_left");
                return;
            }
            if (_account.GateData.Money <= (int)nudMinimumUridium.Value)
            {
                Stopping_gate_mode("minimum_uridium_reached");
                return;
            }
            if (chkSpinOnlyEE.Checked && _account.GateData.Samples <= 0)
            {
                Stopping_gate_mode("no_ee_left");
                return;
            }

            if (rbBuildABG.Checked)
                Log("Spinning ABG...");
            else if (rbBuildKuiper.Checked)
                Log("Spinning Kuiper...");
            else
                Log($"Spinning {GetSelectedGate().GetFullName()}...");

            var multiplierinfo = _account.GateData.MultiplierInfo;
            foreach (var mi in multiplierinfo.MultiplierInfo)
            {
                if (mi.Mode.Contains(GetSelectedGate().GetFullName().ToLower()))
                {
                    if (chkBoxSmartMultiplier.Checked) {
                        if (rbBuildABG.Checked)
                        {
                            if (use3MultiplierABG(_account.GateData.Gates.Get(GalaxyGate.Alpha).Current, _account.GateData.Gates.Get(GalaxyGate.Alpha).Total, _account.GateData.Gates.Get(GalaxyGate.Beta).Current, _account.GateData.Gates.Get(GalaxyGate.Beta).Total, _account.GateData.Gates.Get(GalaxyGate.Beta).Current, _account.GateData.Gates.Get(GalaxyGate.Beta).Total))
                            {
                                if (2 <= mi.Value)
                                {
                                    useMultiplier = true;
                                }
                                else
                                {
                                    useMultiplier = false;
                                }
                            }
                            else
                            {
                                if (1 <= mi.Value)
                                {
                                    useMultiplier = true;
                                }
                                else
                                {
                                    useMultiplier = false;
                                }
                            }
                        }
                        else
                        {
                            if ((_account.GateData.Gates.Get(GetSelectedGate()).Total - _account.GateData.Gates.Get(GetSelectedGate()).Current) == 3)
                            {
                                if (2 <= mi.Value)
                                {
                                    useMultiplier = true;
                                }
                                else
                                {
                                    useMultiplier = false;
                                }
                            }
                            else
                            {
                                if (1 <= mi.Value)
                                {
                                    useMultiplier = true;
                                }
                                else
                                {
                                    useMultiplier = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (getMulplier() <= mi.Value)
                        {
                            useMultiplier = true;
                        }
                        else
                        {
                            useMultiplier = false;
                        }
                    }
                }
            }

            var spin = await _account.SpinGateAsync(GetSelectedGate(), useMultiplier, getEnergyCharge());            

            foreach (var allItem in spin.Items.GetAllItems())
            {
                if (allItem.Duplicate)
                {
                    Log($"Received duplicate gate part > received multiplier");
                }
                else
                {
                    Log($"Received {allItem.ToString()}");
                }
            }

            UpdateGateGui();
        }

        private async Task DoWork()
        {
            try
            {
                Log("Reading Galaxy Gates...");
                await _account.ReadGatesAsync();
                UpdateGateGui();

                while (_running)
                {
                    try
                    {
                        if (chkBoxBuildTechs.Checked)
                        {
                            if (DateTime.Now.Subtract(_nextRunTechFactory).TotalSeconds >= 0)
                            {
                                Log("Executing Tech Factory Task...");
                                await ExecuteTechFactoryAsync();
                                _nextRunTechFactory = DateTime.Now.AddMinutes((int) nudCheckTechFactoryEvery.Value);
                                Log($"Next Tech Factory run -> {_nextRunTechFactory.ToLongTimeString()}");
                            }
                        }

                        if (chkBoxUpgradeSkylab.Checked)
                        {
                            if (DateTime.Now.Subtract(_nextRunSkylab).TotalSeconds >= 0)
                            {
                                Log("Executing Skylab Task...");
                                await ExecuteSkylabAsync();
                                _nextRunSkylab = DateTime.Now.AddMinutes((int) nudCheckSkylabEvery.Value);
                                Log($"Next Skylab run -> {_nextRunSkylab.ToLongTimeString()}");
                            }
                        }

                        if (chkBoxSpinGate.Checked)
                        {
                            if (DateTime.Now.Subtract(_nextRunGalaxyGate).TotalSeconds >= 0)
                            {
                                Log($"Sleeping {(int) nudGateDelay.Value} ms...");
                                await Task.Delay((int) nudGateDelay.Value);
                                await ExecuteSpinAsync();
                            }
                        }

                        if (DateTime.Now.Subtract(_nextRefreshGalaxyGate).TotalSeconds >= 0)
                        {
                            _nextRefreshGalaxyGate = _nextRefreshGalaxyGate.AddMinutes(2);
                            Log("Refreshing Galaxy Gate...");
                            await _account.ReadGatesAsync();
                            UpdateGateGui();

                        }

                        var delay = GetMinimumWaitingTime();
                        if (delay.TotalMilliseconds > 0)
                        {
                            Log($"Sleeping {delay.FormatReadable()} ...");
                            await Task.Delay(delay, _cancellationTokenSource.Token);
                        }

                    }
                    catch (InvalidSessionException)
                    {
                        //reconnect
                        Log($"Account kicked out!");
                        var reconnectSuccess = false;
                        if (_account.AccountData.UsernamePasswordLogin && chkBoxReconnect.Checked)
                        {
                            Log($"Trying to reconnect...");

                            for (var i = 0; i < 3; i++)
                            {
                                Log($"Trying to reconnect {i + 1}...");
                                await Task.Delay(5000);
                                if (await _account.LoginAsync())
                                {
                                    Log($"Reconnect success!");
                                    reconnectSuccess = true;
                                    break;
                                }
                            }
                        }

                        //reconnect failed 3 times
                        if (!reconnectSuccess)
                        {
                            Log($"Reconnect failed!");
                            DisableGui();
                            tabPageLogin.Enabled = true;
                            break;
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception e)
            {
                Log("RunTask: " + e.ToString());
            }
            finally
            {
                _running = false;
                cmdStart.Invoke(new Action(() => cmdStart.Enabled = true));
                Log("RunTask destroyed...Logic stopped...");

                if (chkBoxActivateTheNotification.Checked)
                {
                    notifyIcon1.Icon = this.Icon;
                    notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
                    notifyIcon1.BalloonTipTitle = "The process has been stopped";
                    notifyIcon1.BalloonTipText = "RunTask destroyed...Logic stopped...";
                    notifyIcon1.ShowBalloonTip(15);
                }

                if (chkBoxStartAgainTaskDestroy.Checked && !manuallyStopped)
                {
                    Thread.Sleep(5000);
                    _running = true;
                    _runTask = Task.Run(DoWork, _cancellationTokenSource.Token);
                }
                else
                {
                    _runTask = null;
                }
            }
        }

        private TimeSpan GetMinimumWaitingTime()
        {
            var dateTimes = new List<DateTime>();
            if (chkBoxSpinGate.Checked)
                dateTimes.Add(_nextRunGalaxyGate);
            if (chkBoxUpgradeSkylab.Checked)
                dateTimes.Add(_nextRunSkylab);
            if (chkBoxBuildTechs.Checked)
                dateTimes.Add(_nextRunTechFactory);
            dateTimes.Add(_nextRefreshGalaxyGate);

            return TimeSpan.FromMilliseconds(dateTimes.Min().Subtract(DateTime.Now).TotalMilliseconds + 1000);
        }

        private void rbUsernamePasswordLogin_CheckedChanged(object sender, EventArgs e)
        {
            rbSessionIdLogin.Checked = !rbUsernamePasswordLogin.Checked;
        }

        private void rbSessionIdLogin_CheckedChanged(object sender, EventArgs e)
        {
            rbUsernamePasswordLogin.Checked = !rbSessionIdLogin.Checked;
        }

        private async void cmdLogin_Click(object sender, EventArgs e)
        {
            DisableGui();
            if (rbUsernamePasswordLogin.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Please enter your username and password!");
                    EnableGui();
                    return;
                }
                Log("Performing username/password login...");
                _account = new DarkOrbitAccount(txtUsername.Text, txtPassword.Text, comboBoxLoginPortal.SelectedItem.ToString());
                var loginSuccess = await _account.LoginAsync();

                if (!loginSuccess)
                {
                    Log("There was a problem performing the login! Please check your input data!");
                    MessageBox.Show("There was a problem performing the login! Please check your input data!");
                    EnableGui();
                    return;
                } else
                {
                    loginAccount = true;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtServer.Text) || string.IsNullOrWhiteSpace(txtServer.Text))
                {
                    MessageBox.Show("Please enter your server and sessionId!");
                    EnableGui();
                    return;
                }
                Log("Performing sessionId login...");
                _account = new DarkOrbitAccount(txtServer.Text, txtSessionId.Text);
                var sessionValid = await _account.CheckSessionValidAsync();
                if (!sessionValid)
                {
                    Log("There was a problem performing the login! Please check your input data!");
                    MessageBox.Show("There was a problem performing the login! Please check your input data!");
                    EnableGui();
                    return;
                } else
                {
                    loginAccount = true;
                }
            }

            if (!chkBoxHideName.Checked)
            {
                Text = "Corvus - DarkOrbit Bot Helper - " + _account.AccountData.Username;
            }
            else
            {
                Text = "Corvus - DarkOrbit Bot Helper";
            }

            Log("Login success!");

            await InitializeGuiAsync();

            EnableGui();
        }

        private void cmdResetGateStats_Click(object sender, EventArgs e)
        {
            _account.GateItemsReceived.Reset();
            UpdateGateGui();
        }

        private void cmdStart_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _running = true;
            cmdStart.Enabled = false;
            cmdStop.Enabled = true;
            rbBuildABG.Enabled = false;
            rbBuildDelta.Enabled = false;
            rbBuildEpsilon.Enabled = false;
            rbBuildZeta.Enabled = false;
            rbBuildKappa.Enabled = false;
            rbBuildLambda.Enabled = false;
            rbBuildHades.Enabled = false;
            rbBuildKuiper.Enabled = false;
            _runTask = Task.Run(DoWork, _cancellationTokenSource.Token);
        }

        private void cmdStop_Click(object sender, EventArgs e)
        {
            manuallyStopped = true;
            _cancellationTokenSource.Cancel();
            _running = false;
            cmdStop.Enabled = false;
            rbBuildABG.Enabled = true;
            rbBuildDelta.Enabled = true;
            rbBuildEpsilon.Enabled = true;
            rbBuildZeta.Enabled = true;
            rbBuildKappa.Enabled = true;
            rbBuildLambda.Enabled = true;
            rbBuildHades.Enabled = true;
            rbBuildKuiper.Enabled = true;
        }

        private void cmdOpenBackPage_Click(object sender, EventArgs e)
        {
            Process.Start(string.Format(Urls.OpenBackPage, Urls.BaseUrl, _account.AccountData.SessionId));
        }

        private void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                var dgv = (DataGridView)sender;
                dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
                if (dgv.CurrentCell == null)
                    return;
                if (dgv.CurrentCell.EditType != typeof(DataGridViewTextBoxEditingControl))
                    return;
                dgv.BeginEdit(false);
                var textBox = (TextBox)dgv.EditingControl;
                textBox.SelectionStart = textBox.Text.Length;
            }
            catch (Exception)
            {
            }

        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            ((DataGridView) sender).ClearSelection();
        }

        public void Log(string text, Color color = new Color())
        {
            if (lblLastStatus.InvokeRequired)
            {
                lblLastStatus.Invoke(new Action(() => lblLastStatus.Text = $"Last status: {text}"));
            }
            else
            {
                lblLastStatus.Text = $"Last status: {text}";
            }

            if (rtbLog.InvokeRequired && chkBoxActLog.Checked)
            {
                rtbLog.Invoke(new Action(() => Log(text, color)));
                return;
            }

            var dt = $"[{DateTime.Now:HH:mm:ss}] ";

            rtbLog.SelectionStart = rtbLog.Text.Length;
            rtbLog.SelectionColor = color;
            if (chkBoxActLog.Checked)
            {
                if (rtbLog.Lines.Length == 0)
                {
                    rtbLog.AppendText(dt + text);
                    rtbLog.ScrollToCaret();
                    rtbLog.AppendText(Environment.NewLine);
                }
                else
                {
                    rtbLog.AppendText(dt + text + Environment.NewLine);
                    rtbLog.ScrollToCaret();
                }
            }
        }

        private void SaveSettings(bool confirm = false)
        {
            try
            {
                var iniFile = new FileIniDataParser();
                if (File.Exists("settings.ini"))
                {
                    File.Delete("settings.ini");
                }

                var iniData = new IniData();

                if (chkBoxSaveUsernamePassword.Checked)
                {
                    iniData["Account"]["Username"] = txtUsername.Text;
                    try
                    {
                        iniData["Account"]["Password"] = Encryption.Encrypt(txtPassword.Text);
                    }
                    catch (Exception)
                    {
                        iniData["Account"]["Password"] = string.Empty;
                    }
                    
                    iniData["Account"]["Portal"] = comboBoxLoginPortal.SelectedItem.ToString();
                }
                else
                {
                    iniData["Account"]["Username"] = string.Empty;
                    iniData["Account"]["Password"] = string.Empty;
                    iniData["Account"]["Portal"] = string.Empty;
                }

                iniData["Login"]["UsernamePasswordLogin"] = rbUsernamePasswordLogin.Checked.ToString();
                iniData["Login"]["SaveUsernamePassword"] = chkBoxSaveUsernamePassword.Checked.ToString();
                iniData["Login"]["Reconnect"] = chkBoxReconnect.Checked.ToString();
                iniData["GeneralSettings"]["ActivateTheNotification"] = chkBoxActivateTheNotification.Checked.ToString();
                iniData["GeneralSettings"]["StartAgainTaskDestroy"] = chkBoxStartAgainTaskDestroy.Checked.ToString();
                iniData["GeneralSettings"]["ActLog"] = chkBoxActLog.Checked.ToString();
                iniData["GeneralSettings"]["HideName"] = chkBoxHideName.Checked.ToString();

                iniData["GalaxyGates"]["Spin"] = chkBoxSpinGate.Checked.ToString();
                iniData["GalaxyGates"]["Wait"] = nudGateWait.Text;
                iniData["GalaxyGates"]["Delay"] = nudGateDelay.Text;
                iniData["GalaxyGates"]["MinUridium"] = nudMinimumUridium.Text;
                iniData["GalaxyGates"]["EnergyBoost"] = comboBoxEnergyBoost.SelectedIndex.ToString();
                iniData["GalaxyGates"]["SmartMultiplier"] = chkBoxSmartMultiplier.Checked.ToString();
                iniData["GalaxyGates"]["Multiplier"] = comboBoxMultiplier.SelectedIndex.ToString();
                iniData["GalaxyGates"]["OptionABG"] = comboBoxOptionABG.SelectedIndex.ToString();
                iniData["GalaxyGates"]["OnlyEE"] = chkSpinOnlyEE.Checked.ToString();
                iniData["GalaxyGates"]["SelectedGate"] = GetSelectedGate().GetFullName();
                iniData["GalaxyGates"]["PlaceGate"] = chkBoxPlaceGate.Checked.ToString();

                iniData["TechFactory"]["Build"] = chkBoxBuildTechs.Checked.ToString();
                iniData["TechFactory"]["SleepTime"] = nudCheckTechFactoryEvery.Text;
                iniData["TechFactory"]["BuildTech1"] = _techHall1Row.Cells[_dgvTechFactoryBuild].Value == null ? "False" : _techHall1Row.Cells[_dgvTechFactoryBuild].Value.ToString();
                iniData["TechFactory"]["BuildTech2"] = _techHall2Row.Cells[_dgvTechFactoryBuild].Value == null ? "False" : _techHall2Row.Cells[_dgvTechFactoryBuild].Value.ToString();
                iniData["TechFactory"]["BuildTech3"] = _techHall3Row.Cells[_dgvTechFactoryBuild].Value == null ? "False" : _techHall3Row.Cells[_dgvTechFactoryBuild].Value.ToString();

                iniData["Skylab"]["Upgrade"] = chkBoxUpgradeSkylab.Checked.ToString();
                iniData["Skylab"]["SleepTime"] = nudCheckSkylabEvery.Text;
                iniData["Skylab"]["UpgradePrometiumCollector"] = _prometiumCollectorRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _prometiumCollectorRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeEnduriumCollector"] = _enduriumCollectorRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _enduriumCollectorRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeTerbiumCollector"] = _terbiumCollectorRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _terbiumCollectorRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeSolarModule"] = _solarModuleRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _solarModuleRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeStorageModule"] = _storageModuleRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _storageModuleRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeBaseModule"] = _baseModuleRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _baseModuleRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradePrometidRefinery"] = _prometidRefineryRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _prometidRefineryRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeDuraniumRefinery"] = _duraniumRefineryRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _duraniumRefineryRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradePromeriumRefinery"] = _promeriumRefineryRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _promeriumRefineryRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeXenoModule"] = _xenoModuleRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _xenoModuleRow.Cells[_dgvSkylabUpgrade].Value.ToString();
                iniData["Skylab"]["UpgradeSepromRefinery"] = _sepromRefineryRow.Cells[_dgvSkylabUpgrade].Value == null ? "False" : _sepromRefineryRow.Cells[_dgvSkylabUpgrade].Value.ToString();


                iniFile.WriteFile("settings.ini", iniData);

                if (confirm)
                {
                    MessageBox.Show("Setting saved!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error while saving! Please try to start as administrator!", "Error while saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                var iniFile = new FileIniDataParser();
                if (!File.Exists("settings.ini"))
                {
                    return;
                }

                var iniData = iniFile.ReadFile("settings.ini");


                txtUsername.Text = iniData["Account"]["Username"];
                try
                {
                    txtPassword.Text = Encryption.Decrypt(iniData["Account"]["Password"]);
                }
                catch (Exception)
                {
                }
                
                comboBoxLoginPortal.SelectedIndex = comboBoxLoginPortal.Items.IndexOf(iniData["Account"]["Portal"]);


                rbUsernamePasswordLogin.Checked = bool.Parse(iniData["Login"]["UsernamePasswordLogin"]);
                chkBoxSaveUsernamePassword.Checked = bool.Parse(iniData["Login"]["SaveUsernamePassword"]);
                chkBoxReconnect.Checked = bool.Parse(iniData["Login"]["Reconnect"]);
                chkBoxActivateTheNotification.Checked = bool.Parse(iniData["GeneralSettings"]["ActivateTheNotification"]);
                chkBoxStartAgainTaskDestroy.Checked = bool.Parse(iniData["GeneralSettings"]["StartAgainTaskDestroy"]);
                chkBoxActLog.Checked = bool.Parse(iniData["GeneralSettings"]["ActLog"]);
                chkBoxHideName.Checked = bool.Parse(iniData["GeneralSettings"]["HideName"]);

                chkBoxSpinGate.Checked = bool.Parse(iniData["GalaxyGates"]["Spin"]);
                nudGateWait.Value = decimal.Parse(iniData["GalaxyGates"]["Wait"]);
                nudGateDelay.Value = decimal.Parse(iniData["GalaxyGates"]["Delay"]);
                comboBoxEnergyBoost.SelectedIndex = int.Parse(iniData["GalaxyGates"]["EnergyBoost"]);
                chkBoxSmartMultiplier.Checked = bool.Parse(iniData["GalaxyGates"]["SmartMultiplier"]);
                comboBoxMultiplier.SelectedIndex = int.Parse(iniData["GalaxyGates"]["Multiplier"]);
                nudMinimumUridium.Value = decimal.Parse(iniData["GalaxyGates"]["MinUridium"]);
                comboBoxOptionABG.SelectedIndex = int.Parse(iniData["GalaxyGates"]["OptionABG"]);
                chkSpinOnlyEE.Checked = bool.Parse(iniData["GalaxyGates"]["OnlyEE"]);
                chkBoxPlaceGate.Checked = bool.Parse(iniData["GalaxyGates"]["PlaceGate"]);

                switch (iniData["GalaxyGates"]["SelectedGate"].GalaxyGateFromFullName())
                {
                    default:
                    case GalaxyGate.Alpha:
                        rbBuildABG.Checked = true;
                        break;
                    case GalaxyGate.Delta:
                        rbBuildDelta.Checked = true;
                        break;
                    case GalaxyGate.Epsilon:
                        rbBuildEpsilon.Checked = true;
                        break;
                    case GalaxyGate.Zeta:
                        rbBuildZeta.Checked = true;
                        break;
                    case GalaxyGate.Kappa:
                        rbBuildKappa.Checked = true;
                        break;
                    case GalaxyGate.Lambda:
                        rbBuildLambda.Checked = true;
                        break;
                    case GalaxyGate.Hades:
                        rbBuildHades.Checked = true;
                        break;
                    case GalaxyGate.Kuiper:
                        rbBuildKuiper.Checked = true;
                        break;
                }
                

                chkBoxBuildTechs.Checked = bool.Parse(iniData["TechFactory"]["Build"]);
                nudCheckTechFactoryEvery.Value = decimal.Parse(iniData["TechFactory"]["SleepTime"]);
                _techHall1Row.Cells[_dgvTechFactoryBuild].Value = bool.Parse(iniData["TechFactory"]["BuildTech1"]);
                _techHall2Row.Cells[_dgvTechFactoryBuild].Value = bool.Parse(iniData["TechFactory"]["BuildTech2"]);
                _techHall3Row.Cells[_dgvTechFactoryBuild].Value = bool.Parse(iniData["TechFactory"]["BuildTech3"]);

                chkBoxUpgradeSkylab.Checked = bool.Parse(iniData["Skylab"]["Upgrade"]);
                nudCheckSkylabEvery.Value = decimal.Parse(iniData["Skylab"]["SleepTime"]);
                _prometiumCollectorRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradePrometiumCollector"]);
                _enduriumCollectorRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeEnduriumCollector"]);
                _terbiumCollectorRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeTerbiumCollector"]);
                _solarModuleRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeSolarModule"]);
                _storageModuleRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeStorageModule"]);
                _baseModuleRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeBaseModule"]);
                _prometidRefineryRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradePrometidRefinery"]);
                _duraniumRefineryRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeDuraniumRefinery"]);
                _promeriumRefineryRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradePromeriumRefinery"]);
                _xenoModuleRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeXenoModule"]);
                _sepromRefineryRow.Cells[_dgvSkylabUpgrade].Value = bool.Parse(iniData["Skylab"]["UpgradeSepromRefinery"]);
            }
            catch (Exception)
            {
                MessageBox.Show("Error while loading settings! Please try to start as administrator!", "Error while loading settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmdSaveSettings_Click(object sender, EventArgs e)
        {
            SaveSettings(true);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private String getOptionforABG()
        {
            switch (GetSelecetedIndex_comboBoxOptionABG())
            {
                case 0:
                    return "option1";
                case 1:
                    return "option2";
                case 2:
                    return "option3";
                case 3:
                    return "option4";
                case 4:
                    return "option5";
                default:
                    return "null";
            }
        }

        public void setGatesBuilt(GalaxyGate gate)
        {
            switch ((int) gate)
            {
                case 1:
                    GatesBuiltAlpha++;
                    break;
                case 2:
                    GatesBuiltBeta++;
                    break;
                case 3:
                    GatesBuiltGamma++;
                    break;
                case 4:
                    GatesBuiltDelta++;
                    break;
                case 5:
                    GatesBuiltEpsilon++;
                    break;
                case 6:
                    GatesBuiltZeta++;
                    break;
                case 7:
                    GatesBuiltKappa++;
                    break;
                case 8:
                    GatesBuiltLambda++;
                    break;
                case 13:
                    GatesBuiltHades++;
                    break;
                case 19:
                    GatesBuiltKuiper++;
                    break;
            }
        }

        public async Task PlaceGateAsync(GalaxyGate gate)
        {
            Log($"Gate {gate.GetFullName()} is ready...");
            Log($"Placing {gate.GetFullName()}");
            await _account.PlaceGateAsync(gate);
            setGatesBuilt(gate);
            Log("Reading Galaxy Gates...");
            await _account.ReadGatesAsync();
            UpdateGateGui();
        }

        public void Stopping_gate_mode(String value)
        {
            switch(value)
            {
                case "can_not_get_more_parts":
                    Log($"Stopping gate mode for {nudGateWait.Value} minutes... Can not get more parts");
                    break;
                case "no_uridium/ee_left":
                    Log($"Stopping gate mode for {nudGateWait.Value} minutes... No Uridium/EE left");
                    break;
                case "minimum_uridium_reached":
                    Log($"Stopping gate mode for {nudGateWait.Value} minutes... Minimum Uridium reached");
                    break;
                case "no_ee_left":
                    Log($"Stopping gate mode for {nudGateWait.Value} minutes... No EE left");
                    break;
            }
            _nextRunGalaxyGate = DateTime.Now.AddMinutes((double)nudGateWait.Value);
        }

        private int getMulplier()
        {
            switch (GetSelecetedIndex_comboBoxMultiplier())
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                case 2:
                    return 3;
                case 3:
                    return 4;
                case 4:
                    return 5;
                default:
                    return 1;
            }
        }

        private int GetSelecetedIndex_comboBoxOptionABG()
        {
            if (comboBoxOptionABG.InvokeRequired)
                return (int)comboBoxOptionABG.Invoke(new Func<int>(GetSelecetedIndex_comboBoxOptionABG));
            else
                return comboBoxOptionABG.SelectedIndex;
        }

        private int GetSelecetedIndex_comboBoxMultiplier()
        {
            if (comboBoxMultiplier.InvokeRequired)
                return (int)comboBoxMultiplier.Invoke(new Func<int>(GetSelecetedIndex_comboBoxMultiplier));
            else
                return comboBoxMultiplier.SelectedIndex;
        }

        private void ChkBoxHideName_CheckedChanged(object sender, EventArgs e)
        {
            if (loginAccount){
                if (chkBoxHideName.Checked)
                {
                    Text = "Corvus - DarkOrbit Bot Helper";
                }
                else
                {
                    Text = "Corvus - DarkOrbit Bot Helper - " + _account.AccountData.Username;
                }
            }
        }

        private void ChkBoxMultiplierInt_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxSmartMultiplier.Checked)
            {
                lblMultiplier.Enabled = false;
                comboBoxMultiplier.Enabled = false;
            }
            else
            {
                lblMultiplier.Enabled = true;
                comboBoxMultiplier.Enabled = true;
            }
        }

        private void RbBuildABG_CheckedChanged(object sender, EventArgs e)
        {
            if (rbBuildABG.Checked)
            {
                comboBoxOptionABG.Enabled = true;
            }
            else
            {
                comboBoxOptionABG.Enabled = false;
            }
        }

        private bool use3MultiplierABG(int a_curr, int a_max, int b_curr, int b_max, int g_curr, int g_max)
        {
            if (a_max - a_curr == 3) return b_curr <= b_max * 0.2 && g_curr <= g_max * 0.2;
            if (b_max - b_curr == 3) return a_curr <= a_max * 0.2 && g_curr <= g_max * 0.2;
            if (g_max - b_curr == 3) return a_curr <= a_max * 0.2 && b_curr <= b_max * 0.2;
            return false;
        }

        private void ChkBoxActLog_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBoxActLog.Checked)
            {
                tcMain.TabPages.Insert(4, tabPageLog);
            }
            else
            {
                tcMain.TabPages.Remove(tabPageLog);
            }
        }
        private int getEnergyCharge()
        {
            switch (GetSelecetedIndex_comboBoxEnergyCharge())
            {
                case 0:
                    return 1;
                case 1:
                    return 5;
                case 2:
                    return 10;
                case 3:
                    return 100;
                default:
                    return 1;
            }
        }

        private int GetSelecetedIndex_comboBoxEnergyCharge()
        {
            if (comboBoxEnergyBoost.InvokeRequired)
                return (int)comboBoxEnergyBoost.Invoke(new Func<int>(GetSelecetedIndex_comboBoxEnergyCharge));
            else
                return comboBoxEnergyBoost.SelectedIndex;
        }
    }
}
