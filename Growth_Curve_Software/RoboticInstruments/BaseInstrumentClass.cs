﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Reflection;
namespace Clarity
{
    
    [Serializable]
    public abstract class BaseInstrumentClass
    {
        /// <summary>
        /// This variable indicates whether the instrument is able to 
        /// perform commands
        /// </summary>
        public bool StatusOK = false;

        /// <summary>
        /// The name of the instrument, should be unique
        /// </summary>
        public virtual string Name
        {
            get{return this.GetType().Name;} 
        }
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// This is an optional method that allows for some instruments to try
        /// and self-diagnos and correct a mistake based on the error produced.
        /// </summary>
        /// <param name="Error"></param>
        /// <returns></returns>
        public abstract bool AttemptRecovery(InstrumentError Error);
        //this method should absolutely attempt to resolve the issue, if it cannot, then it should return a value indicating as much
        public virtual void Initialize()
        {
            return;
        }
        public virtual void Initialize(int Parameter)
        {
            return;
        }
        [UserCallableMethod()]
        public virtual bool AttemptRecovery()
        {
            return AttemptRecovery(new InstrumentError("none", false, this));
        }
        //this method should initialize the instrument and return a status okay
        public abstract bool CloseAndFreeUpResources();
        /// <summary>
        /// This is a method called to force certain instruments to free up resources that they
        /// are connecting to in a seperate process
        /// </summary>
        /// <param name="ProcessNameWithoutExeEnding">The name of the instrument to kill</param>
        public static void KillProcessAttempt(string ProcessNameWithoutExeEnding)
        {
            try
            {
                Process[] processList = Process.GetProcesses();
                var x = from p in processList select p.ProcessName;
                if (x.Contains(ProcessNameWithoutExeEnding))
                {
                    string output = String.Empty;
                    System.Diagnostics.Process proc = new Process();
                    ProcessStartInfo myStartInfo = new ProcessStartInfo();
                    myStartInfo.RedirectStandardInput = false;
                    myStartInfo.UseShellExecute = false;
                    myStartInfo.RedirectStandardOutput = false;
                    myStartInfo.Arguments = "/IM " + ProcessNameWithoutExeEnding + ".exe";
                    myStartInfo.CreateNoWindow = true;
                    myStartInfo.FileName = @"C:\WINDOWS\SYSTEM32\TASKKILL.EXE ";
                    proc.StartInfo = myStartInfo;

                    proc.Start();
                    proc.WaitForExit(3000);
                    output = proc.StandardOutput.ReadToEnd();

                }
            }
            catch { }
        } 
        /// <summary>
        /// This is a generic method that will set any instance variables based on 
        /// the xml node that corresponds to the instrument, in this case it does nothing 
        /// but call its initalization method
        /// </summary>
        /// <param name="instrumentNode"></param>
        public virtual void InitializeFromParsedXML(XmlNode instrumentNode)
        {
            //First remove anything old
            
            SetPropertiesByXML(instrumentNode, this);
            this.Initialize();          
        }
        /// <summary>
        /// This is a generic method that will take the initialization values provided by
        /// the xml and use them to set instance variables, the xml childnodes must corresponds to variable
        /// names
        /// </summary>
        /// <param name="xml"></param>
        public static void SetPropertiesByXML(XmlNode nodeToGetValuesFrom, object toSet)
        {
            try
            {
                foreach (XmlNode childNode in nodeToGetValuesFrom)
                {
                    if (toSet == null)
                    {
                        throw new Exception("XML node is being used to set a null variable, the node is \n" + childNode.ToString());

                    }

                    string propertyName = childNode.Name;
                    Type thisType = toSet.GetType();
                    //get the variable type info
                    XmlNode typeNode = childNode.Attributes.RemoveNamedItem("Type");
                    if (typeNode == null)
                    {
                        throw new Exception("Variable Type not set in xml, please declare the variable type for all "
                            + " variables used for " + toSet.ToString());
                    }

                    Type VariableType = System.Type.GetType(typeNode.Value);
                    var Value = Convert.ChangeType(childNode.InnerText, VariableType);
                    //now get the property and change it
                    var prop = thisType.GetProperty(propertyName);
                    if (prop == null)
                    {
                        throw new Exception(toSet.ToString() + " does not have a property called " + propertyName
                            + "\n so the xml file needs to be fixed");
                    }
                    prop.SetValue(toSet, Value, null);
                }
            }
            catch (Exception thrown)
            {
                Exception newExcept=new Exception("Could not instantiate class based on XML " + thrown.Message,thrown);
                throw newExcept;
            }
        }
        public static string GetXMLSettingsFile()
        {
            string direc= Directory.GetCurrentDirectory();
            string file = direc + "\\ConfigurationFile.xml";
            return file;
        }
        public virtual void RegisterEventsWithProtocolManager(ProtocolEventCaller PEC) { }
    
    }
    }


