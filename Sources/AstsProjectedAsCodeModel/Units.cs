//-----------------------------------------------------------------------------
//
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
//-----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;

//^ using Microsoft.Contracts;

namespace Microsoft.Cci.Ast {

  /// <summary>
  /// Represents a .NET assembly.
  /// </summary>
  public abstract class Assembly : Module, IAssembly {

    /// <summary>
    /// Allocates an object that represents a .NET assembly.
    /// </summary>
    /// <param name="name">The name of the unit.</param>
    /// <param name="location">An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty. 
    /// The interpretation depends on the IMetadataHost instance used to resolve references to this unit.</param>
    /// <param name="moduleName">The name of the module containing the assembly manifest. This can be different from the name of the assembly itself.</param>
    /// <param name="assemblyReferences">A list of the assemblies that are referenced by this module.</param>
    /// <param name="moduleReferences">A list of the modules that are referenced by this module.</param>
    /// <param name="resources">A list of named byte sequences persisted with the assembly and used during execution, typically via .NET Framework helper classes.</param>
    /// <param name="files">
    /// A list of the files that constitute the assembly. These are not the source language files that may have been
    /// used to compile the assembly, but the files that contain constituent modules of a multi-module assembly as well
    /// as any external resources. It corresonds to the File table of the .NET assembly file format.
    /// </param>
    protected Assembly(IName name, string location, IName moduleName, IEnumerable<IAssemblyReference> assemblyReferences, IEnumerable<IModuleReference> moduleReferences,
      IEnumerable<IResourceReference> resources, IEnumerable<IFileReference> files)
      : base(name, location, Dummy.Assembly, assemblyReferences, moduleReferences) {
      this.moduleName = moduleName;
      this.resources = resources;
      this.files = files;
    }

    /// <summary>
    /// A list of aliases for the root namespace of the referenced assembly.
    /// </summary>
    public IEnumerable<IName> Aliases {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<IName>(); }
    }

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this assembly.
    /// </summary>
    public IEnumerable<ICustomAttribute> AssemblyAttributes {
      [DebuggerNonUserCode]
      get { return this.attributes.AsReadOnly(); }
    }
    readonly List<ICustomAttribute> attributes = new List<ICustomAttribute>();

    /// <summary>
    /// The identity of the assembly.
    /// </summary>
    public AssemblyIdentity AssemblyIdentity {
      [DebuggerNonUserCode]
      get {
        if (this.assemblyIdentity == null)
          this.assemblyIdentity = UnitHelper.GetAssemblyIdentity(this);
        return this.assemblyIdentity;
      }
    }
    AssemblyIdentity/*?*/ assemblyIdentity;

    /// <summary>
    /// The assembly that contains this module.
    /// </summary>
    public override IAssembly/*?*/ ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this; }
    }

    /// <summary>
    /// Identifies the culture associated with the assembly. Typically specified for sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    public virtual string Culture {
      [DebuggerNonUserCode]
      get { return string.Empty; }
    }

    /// <summary>
    /// Public types defined in other modules making up this assembly and to which other assemblies may refer to via this assembly.
    /// </summary>
    public virtual IEnumerable<IAliasForType> ExportedTypes {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<IAliasForType>(); }
    }

    /// <summary>
    /// A list of the files that constitute the assembly. These are not the source language files that may have been
    /// used to compile the assembly, but the files that contain constituent modules of a multi-module assembly as well
    /// as any external resources. It corresonds to the File table of the .NET assembly file format.
    /// </summary>
    public IEnumerable<IFileReference> Files {
      [DebuggerNonUserCode]
      get { return this.files; }
    }
    readonly IEnumerable<IFileReference> files;

    /// <summary>
    /// A set of bits and bit ranges representing properties of the assembly. The value of <see cref="Flags"/> can be set
    /// from source code via the AssemblyFlags assembly custom attribute. The interpretation of the property depends on the target platform.
    /// </summary>
    public virtual uint Flags {
      [DebuggerNonUserCode]
      get { return 0; } //TODO: get from options or an attribute
    }

    /// <summary>
    /// The kind of metadata stored in the module. For example whether the module is an executable or a manifest resource file.
    /// </summary>
    public override ModuleKind Kind {
      [DebuggerNonUserCode]
      get { return this.EntryPoint.ResolvedMethod == Dummy.Method ? ModuleKind.DynamicallyLinkedLibrary : ModuleKind.ConsoleApplication; } //TODO: obtain it from the compiler options
    }

    /// <summary>
    /// A list of the modules that constitute the assembly.
    /// </summary>
    public IEnumerable<IModule> MemberModules {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<IModule>(); }
    }

    /// <summary>
    /// The identity of the module.
    /// </summary>
    public override ModuleIdentity ModuleIdentity {
      [DebuggerNonUserCode]
      get { return this.AssemblyIdentity; }
    }

    /// <summary>
    /// The name of the module containing the assembly manifest. This can be different from the name of the assembly itself.
    /// </summary>
    public override IName ModuleName {
      [DebuggerNonUserCode]
      get { return this.moduleName; }
    }
    readonly IName moduleName;

    /// <summary>
    /// The public part of the key used to encrypt the SHA1 hash over the persisted form of this assembly . Empty if not specified.
    /// This value is used by the loader to decrypt HashValue which it then compares with a freshly computed hash value to verify the
    /// integrity of the assembly.
    /// </summary>
    public virtual IEnumerable<byte> PublicKey {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<byte>(); } //TODO: get this from an option or attribute
    }

    /// <summary>
    /// The hashed 8 bytes of the public key called public key token of the referenced assembly. This is non empty of the referenced assembly is strongly signed.
    /// </summary>
    public IEnumerable<byte> PublicKeyToken {
      [DebuggerNonUserCode]
      get { return UnitHelper.ComputePublicKeyToken(this.PublicKey); }
    }

    /// <summary>
    /// A list of named byte sequences persisted with the assembly and used during execution, typically via .NET Framework helper classes.
    /// </summary>
    public IEnumerable<IResourceReference> Resources {
      [DebuggerNonUserCode]
      get { return this.resources; }
    }
    readonly IEnumerable<IResourceReference> resources;

    /// <summary>
    /// A list of objects representing persisted instances of pairs of security actions and sets of security permissions.
    /// These apply by default to every method reachable from the module.
    /// </summary>
    public virtual IEnumerable<ISecurityAttribute> SecurityAttributes {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<ISecurityAttribute>(); } //TODO: compute this
    }

    /// <summary>
    /// The version of the assembly.
    /// </summary>
    public virtual Version Version {
      [DebuggerNonUserCode]
      get { return new System.Version(0, 0, 0, 0); } //TODO: obtain from compiler options or custom attributes
    }

    #region IAssemblyReference Members

    IAssembly IAssemblyReference.ResolvedAssembly {
      [DebuggerNonUserCode]
      get { return this; }
    }

    AssemblyIdentity IAssemblyReference.UnifiedAssemblyIdentity {
      [DebuggerNonUserCode]
      get { return this.AssemblyIdentity; }
    }

    #endregion

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this; }
    }

    #endregion

  }

  /// <summary>
  /// A reference to a .NET assembly.
  /// </summary>
  public class ResolvedAssemblyReference : ResolvedModuleReference, IAssemblyReference {

    /// <summary>
    /// Allocates a reference to a .NET assembly.
    /// </summary>
    /// <param name="referencedAssembly">The assembly to reference.</param>
    public ResolvedAssemblyReference(IAssembly referencedAssembly)
      : base(referencedAssembly) {
      this.aliases = IteratorHelper.GetEmptyEnumerable<IName>();
    }

    /// <summary>
    /// A list of aliases for the root namespace of the referenced assembly.
    /// </summary>
    public IEnumerable<IName> Aliases {
      [DebuggerNonUserCode]
      get { return this.aliases; }
    }
    IEnumerable<IName> aliases;

    /// <summary>
    /// The identity of the assembly reference.
    /// </summary>
    public AssemblyIdentity AssemblyIdentity {
      [DebuggerNonUserCode]
      get { return this.ResolvedAssembly.AssemblyIdentity; }
    }

    /// <summary>
    /// Identifies the culture associated with the assembly reference. Typically specified for sattelite assemblies with localized resources.
    /// Empty if not specified.
    /// </summary>
    public string Culture {
      [DebuggerNonUserCode]
      get { return this.ResolvedAssembly.Culture; }
    }

    /// <summary>
    /// Calls the visitor.Visit(IAssemblyReference) method.
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// The hashed 8 bytes of the public key called public key token of the referenced assembly. This is non empty of the referenced assembly is strongly signed.
    /// </summary>
    public IEnumerable<byte> PublicKeyToken {
      [DebuggerNonUserCode]
      get {
        return UnitHelper.ComputePublicKeyToken(this.ResolvedAssembly.PublicKey);
      }
    }

    /// <summary>
    /// The referenced assembly.
    /// </summary>
    public IAssembly ResolvedAssembly {
      [DebuggerNonUserCode]
      get {
        IAssembly/*?*/ result = this.ResolvedUnit as IAssembly;
        //^ assume result != null; //The constructor + immutability guarantees this.
        return result;
      }
    }

    /// <summary>
    /// Returns the identity of the assembly reference to which this assembly reference has been unified.
    /// </summary>
    public AssemblyIdentity UnifiedAssemblyIdentity {
      [DebuggerNonUserCode]
      get { return this.ResolvedAssembly.AssemblyIdentity; }
    }

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    public override UnitIdentity UnitIdentity {
      [DebuggerNonUserCode]
      get {
        return this.AssemblyIdentity;
      }
    }

    /// <summary>
    /// The version of the assembly reference.
    /// </summary>
    public Version Version {
      [DebuggerNonUserCode]
      get { return this.ResolvedAssembly.Version; }
    }

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this; }
    }

    #endregion

  }

  /// <summary>
  /// An object that represents a .NET module.
  /// </summary>
  public abstract class Module : Unit, IModule {

    /// <summary>
    /// Allocates an object that represents a .NET module.
    /// </summary>
    /// <param name="name">The name of the unit.</param>
    /// <param name="location">An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty. 
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.</param>
    /// <param name="containingAssembly">The assembly that contains this module.</param>
    /// <param name="assemblyReferences">A list of the assemblies that are referenced by this module.</param>
    /// <param name="moduleReferences">A list of the modules that are referenced by this module.</param>
    protected Module(IName name, string location, IAssembly containingAssembly, IEnumerable<IAssemblyReference> assemblyReferences, IEnumerable<IModuleReference> moduleReferences)
      : base(name, location) {
      this.containingAssembly = containingAssembly;
      this.assemblyReferences = assemblyReferences;
      this.moduleReferences = moduleReferences;
    }

    /// <summary>
    /// A list of the assemblies that are referenced by this module.
    /// </summary>
    public IEnumerable<IAssemblyReference> AssemblyReferences {
      [DebuggerNonUserCode]
      get { return this.assemblyReferences; }
    }
    readonly IEnumerable<IAssemblyReference> assemblyReferences;

    /// <summary>
    /// The preferred memory address at which the module is to be loaded at runtime.
    /// </summary>
    public ulong BaseAddress {
      [DebuggerNonUserCode]
      get { return 0x400000; } //TODO: allow this to be specified via a compilation flag
    }

    /// <summary>
    /// The assembly that contains this module.
    /// </summary>
    public virtual IAssembly/*?*/ ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this.containingAssembly; }
    }
    IAssembly containingAssembly;

    /// <summary>
    /// Returns a root namepace object that is language specific.
    /// </summary>
    /// <returns></returns>
    protected abstract RootUnitNamespace CreateRootNamespace();
    //^ ensures result.RootOwner == this;

    /// <summary>
    /// Flags that control the behavior of the target operating system. CLI implementations are supposed to ignore this, but some operating system pay attention.
    /// </summary>
    public virtual ushort DllCharacteristics {
      [DebuggerNonUserCode]
      get { return 0; } //TODO: provide compilation flag or attribute to control this
    }

    /// <summary>
    /// The method that will be called to start execution of this executable module. 
    /// </summary>
    public abstract IMethodReference EntryPoint {
      get;
    }

    /// <summary>
    /// The alignment of sections in the module's image file.
    /// </summary>
    public virtual uint FileAlignment {
      [DebuggerNonUserCode]
      get { return 512; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// Returns zero or more strings used in the module. If the module is produced by reading in a CLR PE file, then this will be the contents
    /// of the user string heap. If the module is produced some other way, the method may return an empty enumeration or an enumeration that is a
    /// subset of the strings actually used in the module. The main purpose of this method is to provide a way to control the order of strings in a
    /// prefix of the user string heap when writing out a module as a PE file.
    /// </summary>
    public IEnumerable<string> GetStrings() {
      return IteratorHelper.GetEmptyEnumerable<string>();
    }

    /// <summary>
    /// Returns all of the types in the current module.
    /// </summary>
    public IEnumerable<INamedTypeDefinition> GetAllTypes() {
      if (this.allTypes == null) {
        List<INamedTypeDefinition> result = new List<INamedTypeDefinition>();
        result.Add(this.Compilation.ModuleClass);
        result.Add(this.Compilation.GlobalsClass);
        UnitNamespace.FillInWithTypes(this.UnitNamespaceRoot, result);
        this.allTypes = result.AsReadOnly();
      }
      return this.allTypes;
    }
    IEnumerable<INamedTypeDefinition>/*?*/ allTypes;

    /// <summary>
    /// True if the module contains only IL and is processor independent.
    /// </summary>
    public virtual bool ILOnly {
      [DebuggerNonUserCode]
      get { return true; }
    }

    /// <summary>
    /// The kind of metadata stored in the module. For example whether the module is an executable or a manifest resource file.
    /// </summary>
    public virtual ModuleKind Kind {
      [DebuggerNonUserCode]
      get { return ModuleKind.DynamicallyLinkedLibrary; }
    }

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 8 in 8.0.
    /// </summary>
    public virtual byte LinkerMajorVersion {
      [DebuggerNonUserCode]
      get { return 6; }
    }

    /// <summary>
    /// The first part of a two part version number indicating the version of the linker that produced this module. For example, the 0 in 8.0.
    /// </summary>
    public virtual byte LinkerMinorVersion {
      [DebuggerNonUserCode]
      get { return 0; }
    }

    /// <summary>
    /// The first part of a two part version number indicating the version of the format used to persist this module. For example, the 1 in 1.0.
    /// </summary>
    public byte MetadataFormatMajorVersion {
      [DebuggerNonUserCode]
      get { return this.metadataFormatMajorVersion; }
    }
    readonly byte metadataFormatMajorVersion = 2; //TODO: get from compilation host

    /// <summary>
    /// The second part of a two part version number indicating the version of the format used to persist this module. For example, the 0 in 1.0.
    /// </summary>
    public byte MetadataFormatMinorVersion {
      [DebuggerNonUserCode]
      get { return this.metadataFormatMinorVersion; }
    }
    readonly byte metadataFormatMinorVersion = 0; //TODO: get from compilation host

    /// <summary>
    /// A list of objects representing persisted instances of types that extend System.Attribute. Provides an extensible way to associate metadata
    /// with this module.
    /// </summary>
    public IEnumerable<ICustomAttribute> ModuleAttributes {
      [DebuggerNonUserCode]
      get { return this.attributes.AsReadOnly(); }
      //TODO: run through the namespaces and collect together all of their attributes that
      //specify their target to be a module
    }
    readonly List<ICustomAttribute> attributes = new List<ICustomAttribute>();

    /// <summary>
    /// The identity of the module.
    /// </summary>
    public abstract ModuleIdentity ModuleIdentity {
      get;
    }

    /// <summary>
    /// The name of the module. This can be different from the name if this module is also an assembly.
    /// </summary>
    public virtual IName ModuleName {
      [DebuggerNonUserCode]
      get { return this.Name; }
    }

    /// <summary>
    /// A list of the modules that are referenced by this module.
    /// </summary>
    public IEnumerable<IModuleReference> ModuleReferences {
      [DebuggerNonUserCode]
      get { return this.moduleReferences; }
    }
    readonly IEnumerable<IModuleReference> moduleReferences;

    /// <summary>
    /// A globally unique persistent identifier for this module.
    /// </summary>
    public Guid PersistentIdentifier {
      [DebuggerNonUserCode]
      get { return this.persistentIdentifier; }
    }
    readonly Guid persistentIdentifier = Guid.NewGuid();

    /// <summary>
    /// If set, the module contains instructions or assumptions that are specific to the AMD 64 bit instruction set. Setting this flag to
    /// true also sets Requires64bits to true.
    /// </summary>
    public virtual bool RequiresAmdInstructionSet {
      [DebuggerNonUserCode]
      get { return false; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// If set, the module contains instructions that assume a 32 bit instruction set. For example it may depend on an address being 32 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    public virtual bool Requires32bits {
      [DebuggerNonUserCode]
      get { return false; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// If set, the module contains instructions that assume a 64 bit instruction set. For example it may depend on an address being 64 bits.
    /// This may be true even if the module contains only IL instructions because of PlatformInvoke and COM interop.
    /// </summary>
    public virtual bool Requires64bits {
      [DebuggerNonUserCode]
      get { return false; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// The size of the virtual memory initially committed for the initial process heap.
    /// </summary>
    public virtual ulong SizeOfHeapCommit {
      [DebuggerNonUserCode]
      get { return 0x1000; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// The size of the virtual memory to reserve for the initial process heap.
    /// </summary>
    public virtual ulong SizeOfHeapReserve {
      [DebuggerNonUserCode]
      get { return 0x100000; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// The size of the virtual memory to reserve for the initial thread's stack.
    /// </summary>
    public virtual ulong SizeOfStackReserve {
      [DebuggerNonUserCode]
      get { return 0x100000; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// The size of the virtual memory initially committed for the initial thread's stack.
    /// </summary>
    public virtual ulong SizeOfStackCommit {
      [DebuggerNonUserCode]
      get { return 0x1000; } //TODO: provide an option for setting this
    }

    /// <summary>
    /// Identifies the version of the CLR that is required to load this module or assembly.
    /// </summary>
    public string TargetRuntimeVersion {
      [DebuggerNonUserCode]
      get {
        if (this.targetRuntimeVersion == null) {
          IModule/*?*/ mscorlib = TypeHelper.GetDefiningUnit(this.Compilation.PlatformType.SystemObject.ResolvedType) as IModule;
          if (mscorlib == null)
            this.targetRuntimeVersion = "bad target runtime";
          else
            this.targetRuntimeVersion = mscorlib.TargetRuntimeVersion;
        }
        return this.targetRuntimeVersion;
      }
    }
    string/*?*/ targetRuntimeVersion;

    /// <summary>
    /// True if the instructions in this module must be compiled in such a way that the debugging experience is not compromised.
    /// To set the value of this property, add an instance of System.Diagnostics.DebuggableAttribute to the MetadataAttributes list.
    /// </summary>
    public virtual bool TrackDebugData {
      [DebuggerNonUserCode]
      get { return false; } //TODO: get this from an option or an attribute
    }

    /// <summary>
    /// The identity of the unit.
    /// </summary>
    public override UnitIdentity UnitIdentity {
      [DebuggerNonUserCode]
      get { return this.ModuleIdentity; }
    }

    /// <summary>
    /// A root namespace that contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    public override IRootUnitNamespace UnitNamespaceRoot {
      [DebuggerNonUserCode]
      get {
        if (this.unitNamespaceRoot == null) {
          lock (GlobalLock.LockingObject) {
            if (this.unitNamespaceRoot == null) {
              RootUnitNamespace nsRoot = this.unitNamespaceRoot = this.CreateRootNamespace();
              foreach (CompilationPart compilationPart in this.Compilation.Parts)
                nsRoot.AddNamespaceDeclaration(compilationPart.RootNamespace);
            }
          }
        }
        //^ assume this.unitNamespaceRoot.RootOwner == this;
        //^ assume false; //Boogie bug
        return this.unitNamespaceRoot;
      }
    }
    RootUnitNamespace/*?*/ unitNamespaceRoot;
    //^ invariant this.unitNamespaceRoot == null || this.unitNamespaceRoot.RootOwner == this;


    /// <summary>
    /// A list of other units that are referenced by this unit. 
    /// </summary>
    public override IEnumerable<IUnitReference> UnitReferences {
      [DebuggerNonUserCode]
      get {
        foreach (IAssemblyReference assemblyReference in this.AssemblyReferences)
          yield return assemblyReference;
        foreach (IModuleReference moduleReference in this.ModuleReferences)
          yield return moduleReference;
      }
    }

    /// <summary>
    /// True if the module will be persisted with a list of assembly references that include only tokens derived from the public keys
    /// of the referenced assemblies, rather than with references that include the full public keys of referenced assemblies as well
    /// as hashes over the contents of the referenced assemblies. Setting this property to true is appropriate during development.
    /// When building for deployment it is safer to set this property to false.
    /// </summary>
    public virtual bool UsePublicKeyTokensForAssemblyReferences {
      [DebuggerNonUserCode]
      get { return true; } //TODO: get the value from an option
    }

    /// <summary>
    /// A list of named byte sequences persisted with the module and used during execution, typically via the Win32 API.
    /// A module will define Win32 resources rather than "managed" resources mainly to present metadata to legacy tools
    /// and not typically use the data in its own code.
    /// </summary>
    public IEnumerable<IWin32Resource> Win32Resources {
      [DebuggerNonUserCode]
      get { return this.win32Resources.AsReadOnly(); }
    }
    readonly List<IWin32Resource> win32Resources = new List<IWin32Resource>();

    #region IModuleReference Members

    IAssemblyReference/*?*/ IModuleReference.ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this.ContainingAssembly; }
    }

    IModule IModuleReference.ResolvedModule {
      [DebuggerNonUserCode]
      get { return this; }
    }

    #endregion
  }

  /// <summary>
  /// A reference to a .NET module.
  /// </summary>
  public class ResolvedModuleReference : ResolvedUnitReference, IModuleReference {

    /// <summary>
    /// Allocates a reference to a .NET module.
    /// </summary>
    /// <param name="referencedModule">The module to reference.</param>
    internal ResolvedModuleReference(IModule referencedModule)
      : base(referencedModule) {
    }

    /// <summary>
    /// The Assembly that contains this module. May be null if the module is not part of an assembly.
    /// </summary>
    public IAssemblyReference/*?*/ ContainingAssembly {
      [DebuggerNonUserCode]
      get { return this.ResolvedModule.ContainingAssembly; }
    }

    /// <summary>
    /// Calls the visitor.Visit(IModuleReference) method.
    /// </summary>
    public override void Dispatch(IMetadataVisitor visitor) {
      visitor.Visit(this);
    }

    /// <summary>
    /// Returns the identity of the module reference.
    /// </summary>
    public ModuleIdentity ModuleIdentity {
      [DebuggerNonUserCode]
      get { return this.ResolvedModule.ModuleIdentity; }
    }

    /// <summary>
    /// The referenced module.
    /// </summary>
    public IModule ResolvedModule {
      [DebuggerNonUserCode]
      get {
        IModule/*?*/ result = this.ResolvedUnit as IModule;
        //^ assume result != null; //The constructor+immutability guarantees this.
        return result;
      }
    }

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    public override UnitIdentity UnitIdentity {
      [DebuggerNonUserCode]
      get { return this.ModuleIdentity; }
    }

  }

  /// <summary>
  /// A unit of metadata stored as a single artifact and potentially produced and revised independently from other units.
  /// Examples of units include .NET assemblies and modules, as well C++ object files and compiled headers.
  /// </summary>
  public abstract class Unit : IUnit {

    /// <summary>
    /// Initializes a unit of metadata stored as a single artifact and potentially produced and revised independently from other units.
    /// Examples of units include .NET assemblies and modules, as well C++ object files and compiled headers.
    /// </summary>
    /// <param name="name">The name of the unit.</param>
    /// <param name="location">An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty. 
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to this unit.</param>
    protected Unit(IName name, string location) {
      this.name = name;
      this.location = location;
    }

    /// <summary>
    /// The identity of the assembly corresponding to the target platform contract assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.ContractAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    public AssemblyIdentity ContractAssemblySymbolicIdentity {
      [DebuggerNonUserCode]
      get {
        AssemblyIdentity/*?*/ result = null;
        foreach (IUnit unit in this.UnitReferences) {
          AssemblyIdentity contractId = unit.ContractAssemblySymbolicIdentity;
          if (contractId.Name.Value.Length == 0) continue;
          if (result == null || result.Version < contractId.Version) result = contractId;
        }
        if (result != null) return result;
        Assembly/*?*/ assem = this as Assembly;
        if (assem != null) {
          foreach (INamespaceMember member in assem.UnitNamespaceRoot.GetMembersNamed(this.NameTable.GetNameFor("Microsoft"), false)) {
            INestedUnitNamespace/*?*/ microsoft = member as INestedUnitNamespace;
            if (microsoft == null) continue;
            foreach (INamespaceMember mem in microsoft.GetMembersNamed(this.NameTable.GetNameFor("Contracts"), false)) {
              INestedUnitNamespace/*?*/ microsoftContracts = mem as INestedUnitNamespace;
              if (microsoftContracts == null) continue;
              foreach (INamespaceMember m in microsoftContracts.GetMembersNamed(this.NameTable.GetNameFor("Contract"), false)) {
                if (m is INamespaceTypeDefinition) return assem.AssemblyIdentity;
              }
            }
          }
        }
        return Dummy.Assembly.AssemblyIdentity;
      }
    }


    /// <summary>
    /// An assembly reference corresponding to the target platform core assembly at the time this unit was compiled.
    /// This property will be used to implement IMetadataHost.CoreAssemblySymbolicIdentity and its implementation must
    /// consequently not use the latter.
    /// </summary>
    public AssemblyIdentity CoreAssemblySymbolicIdentity {
      [DebuggerNonUserCode]
      get {
        AssemblyIdentity/*?*/ result = null;
        foreach (IUnit unit in this.UnitReferences) {
          AssemblyIdentity coreId = unit.CoreAssemblySymbolicIdentity;
          if (coreId.Name.Value.Length == 0) continue;
          if (result == null || result.Version < coreId.Version) result = coreId;
        }
        if (result != null) return result;
        Assembly/*?*/ assem = this as Assembly;
        if (assem != null) {
          foreach (INamespaceMember member in assem.UnitNamespaceRoot.GetMembersNamed(this.NameTable.System, false)) {
            INestedUnitNamespace/*?*/ system = member as INestedUnitNamespace;
            if (system == null) continue;
            foreach (INamespaceMember mem in system.GetMembersNamed(this.NameTable.Object, false)) {
              if (mem is INamespaceTypeDefinition) return assem.AssemblyIdentity;
            }
          }
        }
        return Dummy.Assembly.AssemblyIdentity;
      }
    }

    /// <summary>
    /// The compilation that produces this unit of metadata.
    /// </summary>
    public abstract Compilation Compilation {
      get;
    }

    /// <summary>
    /// An indication of the location where the unit is or will be stored. This need not be a file system path and may be empty. 
    /// The interpretation depends on the ICompilationHostEnviroment instance used to resolve references to unit.
    /// </summary>
    public string Location {
      [DebuggerNonUserCode]
      get { return this.location; }
    }
    readonly string location;

    /// <summary>
    /// The name of the entity.
    /// </summary>
    public IName Name {
      [DebuggerNonUserCode]
      get {
        return this.name;
        //TODO: if name is the empty name, then look for the main routine and use the name of the source file in which it appears.
        //otherwise just use the name of the first compilation part making up the compilation that resulted in this unit.
        //(Call an abstract helper to do that).
      }
    }
    IName name;

    /// <summary>
    /// A table used to intern strings used as names. This table is obtained from the host environment.
    /// It is mutuable, in as much as it is possible to add new names to the table.
    /// </summary>
    public INameTable NameTable {
      [DebuggerNonUserCode]
      get { return this.Compilation.NameTable; }
    }

    /// <summary>
    /// A collection of well known types that must be part of every target platform and that are fundamental to modeling compiled code.
    /// The types are obtained by querying the unit set of the compilation and thus can include types that are defined by the compilation itself.
    /// </summary>
    public IPlatformType PlatformType {
      [DebuggerNonUserCode]
      get { return this.Compilation.PlatformType; }
    }

    /// <summary>
    /// A root namespace that contains nested namespaces as well as top level types and anything else that implements INamespaceMember.
    /// </summary>
    public abstract IRootUnitNamespace UnitNamespaceRoot {
      get;
      //^ ensures result.RootOwner == this;
    }

    /// <summary>
    /// A list of other units that are referenced by this unit. 
    /// </summary>
    public abstract IEnumerable<IUnitReference> UnitReferences {
      get;
    }

    /// <summary>
    /// Calls visitor.Visit(IUnit).
    /// </summary>
    public abstract void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// Returns the identity of the unit reference.
    /// </summary>
    public abstract UnitIdentity UnitIdentity {
      get;
    }

    #region IUnit Members

    IRootUnitNamespace IUnit.UnitNamespaceRoot {
      [DebuggerNonUserCode]
      get { return this.UnitNamespaceRoot; }
    }

    #endregion

    #region IUnitReference Members

    /// <summary>
    /// The referenced unit, or Dummy.Unit if the reference cannot be resolved.
    /// </summary>
    /// <value></value>
    public IUnit ResolvedUnit {
      [DebuggerNonUserCode]
      get { return this; }
    }

    #endregion

    #region INamespaceRootOwner Members

    INamespaceDefinition INamespaceRootOwner.NamespaceRoot {
      [DebuggerNonUserCode]
      get { return this.UnitNamespaceRoot; }
    }

    #endregion

    #region IReference Members

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    /// <value></value>
    public IEnumerable<ICustomAttribute> Attributes {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<ICustomAttribute>(); }
    }

    /// <summary>
    /// A potentially empty collection of locations that correspond to this instance.
    /// </summary>
    /// <value></value>
    public IEnumerable<ILocation> Locations {
      [DebuggerNonUserCode]
      get {
        foreach (CompilationPart compilationPart in this.Compilation.Parts)
          yield return compilationPart.SourceLocation;
      }
    }

    #endregion

  }

  /// <summary>
  /// A reference to a instance of <see cref="IUnit"/>.
  /// </summary>
  public abstract class ResolvedUnitReference : IUnitReference {

    /// <summary>
    /// Initializes a reference to a instance of <see cref="IUnit"/>.
    /// </summary>
    /// <param name="referencedUnit">The unit to reference.</param>
    internal ResolvedUnitReference(IUnit referencedUnit)
      // ^ ensures this.ResolvedUnit == referencedUnit; //Spec# problem with delayed receiver
    {
      this.resolvedUnit = referencedUnit;
    }

    /// <summary>
    /// A collection of metadata custom attributes that are associated with this definition.
    /// </summary>
    public IEnumerable<ICustomAttribute> Attributes {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<ICustomAttribute>(); }
    }

    /// <summary>
    /// Calls the visitor.Visit(T) method where T is the most derived object model node interface type implemented by the concrete type
    /// of the object implementing IDoubleDispatcher. The dispatch method does not invoke Dispatch on any child objects. If child traversal
    /// is desired, the implementations of the Visit methods should do the subsequent dispatching.
    /// </summary>
    public abstract void Dispatch(IMetadataVisitor visitor);

    /// <summary>
    /// A potentially empty collection of locations that correspond to this IReference instance.
    /// </summary>
    public IEnumerable<ILocation> Locations {
      [DebuggerNonUserCode]
      get { return IteratorHelper.GetEmptyEnumerable<ILocation>(); }
    }

    /// <summary>
    /// The name of the unit.
    /// </summary>
    public IName Name {
      [DebuggerNonUserCode]
      get { return this.UnitIdentity.Name; }
    }

    /// <summary>
    /// The referenced unit.
    /// </summary>
    public IUnit ResolvedUnit {
      [DebuggerNonUserCode]
      get {
        return this.resolvedUnit;
      }
    }
    readonly IUnit resolvedUnit;

    /// <summary>
    /// The identity of the unit reference.
    /// </summary>
    public abstract UnitIdentity UnitIdentity {
      get;
    }

  }

}