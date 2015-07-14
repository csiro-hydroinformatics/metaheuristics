import clr
clr.AddReferenceToFileAndPath(r'C:\src\R\rdotnet\R.NET\bin\Debug\RDotNet.dll')
clr.AddReferenceToFileAndPath(r'C:\src\trunk\Partners\CSIRO\R\CSIRO.TIME2R\bin\Debug\CSIRO.TIME2R.dll')
clr.AddReference("System.Core")
from CSIRO.TIME2R import *
import System
import RDotNet
from RDotNet import *
clr.ImportExtensions(System.Linq)
clr.ImportExtensions(RDotNet) 
clr.ImportExtensions(SymbolicExpressionExtension) 
rdllloc = r"c:\bin\R\R\bin\x64"
REngine.SetDllDirectory(rdllloc)
from System import Environment
rhome = Environment.GetEnvironmentVariable("R_HOME")
Environment.SetEnvironmentVariable("R_HOME", r"c:\bin\R\R")
engine = REngine.CreateInstance("RDotNet", r"c:\bin\R\R\bin\x64\R.dll")
engine.Initialize()
sexp = "data.frame(name=paste('x', 1:4, sep=''), min = rep(0,4), value= rep(1,4), max = rep(2,4), stringsAsFactors=FALSE)"
df = SymbolicExpressionExtension.AsDataFrame(engine.Evaluate(sexp))
hc = TimeToRDataConversion.convert(df)



// .NET Framework array to R vector.
NumericVector group1 = engine.CreateNumericVector(new double[] { 30.02, 29.99, 30.11, 29.97, 30.01, 29.99 });
engine.SetSymbol("group1", group1);
// Direct parsing from R script.
NumericVector group2 = engine.Evaluate("group2 <- c(29.89, 29.93, 29.72, 29.98, 30.02, 29.98)").AsNumeric();
group2[2] = 3.1415;
engine.Evaluate("print(group2)");
engine.Evaluate("group2[3] = group2[3] * 2");

Console.WriteLine(".NET vector group2[2] (third item, that is) is {0}", group2[2]);

// Test difference of mean and get the P-value.
GenericVector testResult = engine.Evaluate("t.test(group1, group2)").AsList();
double p = testResult["p.value"].AsNumeric().First();

Console.WriteLine("Group1: [{0}]", string.Join(", ", group1));
Console.WriteLine("Group2: [{0}]", string.Join(", ", group2));
Console.WriteLine("P-value = {0:0.000}", p);