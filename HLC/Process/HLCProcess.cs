using HLC.DriverComponents;
using HLC.DriverComponents.Entities;
using System;

namespace HLC.Process
{
    internal abstract class HLCProcess
    {
        public abstract string ProcessName { get; }
        public string URL { get; set; }

        public SeleniumBase Selenium;
        public ComponentNode LastNode;
        public ComponentNode CurrentNode;

        protected HLCProcess(SeleniumBase selenium)
        {
            CurrentNode = new ComponentNode();
            Selenium = selenium;
        }

        protected HLCProcess(ComponentNode componentNode, SeleniumBase selenium)
        {
            LastNode = componentNode;
            CurrentNode = new ComponentNode();
            Selenium = selenium; 
        }

        public ComponentNode EstablishRoutine()
        {
            PrepareEnvironment();
            ExtractData();
            SaveData();
            EndProcess();
            return CurrentNode;
        }

        public string DefineProcessName(Type type)
        {
            return type.Name.ToUpper();
        }

        public virtual void PrepareEnvironment()
        {
            try
            {
                Selenium.SetUp();
                Selenium.TryNavigate(URL);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Website loading error: {ex.Message} => {ex.TargetSite.Name} IN {ProcessName}");
            }
        }

        public abstract void ExtractData();

        public abstract void SaveData();

        public abstract void EndProcess();
    }
}
