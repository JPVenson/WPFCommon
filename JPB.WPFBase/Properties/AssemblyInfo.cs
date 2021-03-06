﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("JPB.WPFBase")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Firma")]
[assembly: AssemblyProduct("JPB.WPFBase")]
[assembly: AssemblyCopyright("Copyright © Firma 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("1a126ef6-eb5b-4a66-9eae-e14e7e480291")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: InternalsVisibleTo("JPB.WPFBase.Tests")]

[assembly: XmlnsDefinition("http://schemas.wpftoolsawesome.com/xaml/eval", "Behaviors.Eval")]
[assembly: XmlnsDefinition("http://schemas.wpftoolsawesome.com/xaml/eval/Actions", "Behaviors.Eval.Actions")]
[assembly: XmlnsDefinition("http://schemas.wpftoolsawesome.com/xaml/eval/Evaluators", "Behaviors.Eval.Evaluators")]
[assembly: XmlnsPrefix("http://schemas.wpftoolsawesome.com/xaml/eval", "ev")]
[assembly: XmlnsPrefix("http://schemas.wpftoolsawesome.com/xaml/eval/Actions", "act")]
[assembly: XmlnsPrefix("http://schemas.wpftoolsawesome.com/xaml/eval/Evaluators", "eval")]