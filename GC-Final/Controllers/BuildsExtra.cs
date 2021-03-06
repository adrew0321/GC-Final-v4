﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using GC_Final.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Configuration;
using System.Reflection;

namespace GC_Final.Controllers
{
    [Authorize]
    public partial class BuildsController : Controller
    {
        public ActionResult Create()
        {
            Entities ORM = new Entities();

            ViewBag.GPUs = ORM.GPUs;
            ViewBag.CPUs = ORM.CPUs;
            ViewBag.Motherboards = ORM.Motherboards;
            ViewBag.PSUs = ORM.PSUs;
            ViewBag.RAMs = ORM.RAMs;
            ViewBag.Monitors = ORM.Monitors;
            ViewBag.PCCases = ORM.PCCases;
            ViewBag.HardDrives = ORM.HardDrives;
            ViewBag.OpticalDrivers = ORM.OpticalDrivers;
            ViewBag.PCICards = ORM.PCICards;
            List<string[]> _saveMe = new List<string[]>();
            _saveMe.Add(new string[] { "End", "End" });
            ViewBag.Flags = _saveMe;

            return View();
        }

        [RequireParameter("id")]
        public ActionResult Create(string id)
        {
            Entities ORM = new Entities();
            return _Create(ORM.Builds.Find(id), User.Identity.GetUserId());
        }

        private ActionResult _Create(Build UserBuild, string UID, Dictionary<string, string> Flags)
        {
            ViewBag.Flags = Flags;
            return _Create(UserBuild, UID);
        }

        private ActionResult _Create(Build UserBuild, string UID)
        {
            if (UID == UserBuild.OwnerID)
            {
                Entities ORM = new Entities();

                ViewBag.GPUs = ORM.GPUs;
                ViewBag.CPUs = ORM.CPUs;
                ViewBag.Motherboards = ORM.Motherboards;
                ViewBag.PSUs = ORM.PSUs;
                ViewBag.RAMs = ORM.RAMs;
                ViewBag.Monitors = ORM.Monitors;
                ViewBag.PCCases = ORM.PCCases;
                ViewBag.HardDrives = ORM.HardDrives;
                ViewBag.OpticalDrivers = ORM.OpticalDrivers;
                ViewBag.PCICards = ORM.PCICards;

                ViewBag.SetGPU = UserBuild.GPU;
                ViewBag.SetCPU = UserBuild.CPU;
                ViewBag.SetMotherboard = UserBuild.Motherboard;
                ViewBag.SetPSU = UserBuild.PSU;
                ViewBag.SetRAM = ORM.RAMs.Find(UserBuild.BuildsRAMs.First().RAMID);
                ViewBag.SetMonitor = ORM.Monitors.Find(UserBuild.BuildMonitors.First().MonitorID);
                ViewBag.SetPCCase = UserBuild.PCCase;
                ViewBag.SetHardDrive = ORM.HardDrives.Find(UserBuild.BuildDisks.First().HDID);
                //ViewBag.SetOpticalDriver = ORM.OpticalDrivers.Find(UserBuild.BuildODs.First().ODID);
                //ViewBag.SetPCICard = ORM.PCICards.Find(UserBuild.BuildPCIs.First().PCIID);

                return View("Create");
            }

            return Display(UserBuild.BuildID);
        }
        
        private ActionResult _Verify(string buildName, string motherboard_id, string gpu_id, string cpu_id, string ram_id, string psu_id, string case_id, string monitor_id, string pci_id, string hard_id, string optical_id, bool c)
        {
            bool OVERRIDE = false;
            Entities ORM = new Entities();
            Build temp = new Build();
            temp.BuildName = buildName;
            temp.OwnerID = User.Identity.GetUserId();
            temp.BuildID = Guid.NewGuid().ToString("D");
            temp.Motherboard = ORM.Motherboards.Find(motherboard_id);
            temp.GPU = ORM.GPUs.Find(gpu_id);
            temp.CPU = ORM.CPUs.Find(cpu_id);
            BuildsRAM _ram = new BuildsRAM();
            _ram.BuildID = temp.BuildID;
            _ram.RAMID = ram_id;
            _ram.RAM = ORM.RAMs.Find(ram_id);
            temp.BuildsRAMs.Add(_ram);
            temp.PSU = ORM.PSUs.Find(psu_id);
            temp.PCCase = ORM.PCCases.Find(case_id);
            BuildMonitor _monitor = new BuildMonitor();
            _monitor.BuildID = temp.BuildID;
            _monitor.Monitor = ORM.Monitors.Find(monitor_id);
            _monitor.MonitorID = monitor_id;
            temp.BuildMonitors.Add(_monitor);
            temp.BuildsPeripherals = null;
            BuildPCI _pci = new BuildPCI();
            _pci.BuildID = temp.BuildID;
            _pci.PCICard = ORM.PCICards.Find(pci_id);
            _pci.PCIID = pci_id;
            temp.BuildPCIs.Add(_pci);
            BuildDisk _hd = new BuildDisk();
            _hd.BuildID = temp.BuildID;
            _hd.HardDrive = ORM.HardDrives.Find(hard_id);
            _hd.HDID = hard_id;
            temp.BuildDisks.Add(_hd);
            BuildOD _od = new BuildOD();
            _od.BuildID = temp.BuildID;
            _od.ODID = optical_id;
            _od.OpticalDriver = ORM.OpticalDrivers.Find(optical_id);
            List<string[]> flags = temp.GetCompat();
            OVERRIDE = true;    //Comment this out to re-enable validation before creation.
            if ((flags[0][0] == "End" || OVERRIDE) && c)
            {
                ORM.Builds.Add(temp);
                ORM.SaveChanges();
                return RedirectToAction("Display", "Builds", new { id = temp.BuildID });
            }
            ViewBag.Flags = flags;
            return _Create(temp, User.Identity.GetUserId());
        }

        private int _displayLimit(int count)
        {
            if (count < 15)
            {
                return count % 15;
            }
            else
            {
                return 15;
            }
        }

        [AllowAnonymous]
        private ActionResult _Display()
        {
            Entities ORM = new Entities();
            Random R = new Random();
            Build[] arr = ORM.Builds.ToArray();
            List<Build> _arr = new List<Build>();
            for (int i = 0; i < _displayLimit(arr.Length); i++)
            {
                _arr.Add(arr[R.Next(arr.Length)]);
            }
            ViewBag.UserBuilds = _arr.ToArray();
            return View("Display2");
        }

        [AllowAnonymous]
        public ActionResult Display(string id)
        {
            if (id != null)
            {
                Entities ORM = new Entities();
                ViewBag.Build = ORM.Builds.Find(id);
                if (ViewBag.Build != null)
                {
                    return View();
                }
                return RedirectToAction("Error", new { error = "Invalid Build ID" });
            }
            else
            {
                return _Display();
            }
        }

        [AllowAnonymous]
        [RequireParameter("buildName")]
        public ActionResult Display(string buildName, string motherboard_id, string gpu_id, string cpu_id, string ram_id, string psu_id, string case_id, string monitor_id, string pci_id, string hard_id, string optical_id, bool c)
        {
            return _Verify(buildName, motherboard_id, gpu_id, cpu_id, ram_id, psu_id, case_id, monitor_id, pci_id, hard_id, optical_id, c);
        }
    }
}