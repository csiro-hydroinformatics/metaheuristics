using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CSIRO.Metaheuristics.Logging")]
[assembly: AssemblyDescription("Facilities to log an optimisation process or results.")]
[assembly: AssemblyProduct("CSIRO.Metaheuristics.Logging")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Log4Net.config", Watch = true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c91647bc-7db9-44be-ae9f-2115b3b13b5b")]
