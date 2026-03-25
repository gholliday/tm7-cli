using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Tm7.Cli.Model;

namespace Tm7.Cli;

/// <summary>
/// AOT-safe wrapper for TM7 XML serialization.
/// Encapsulates DataContractSerializer usage behind proper trimming/AOT annotations
/// so the rest of the application remains free of IL2026/IL3050 warnings.
/// All types required for reflection-based serialization are preserved via
/// DynamicDependency attributes and the TrimmerRoots.xml ILLink descriptor.
/// </summary>
public static class Tm7XmlSerializer
{
    private static readonly Type[] KnownTypes =
    [
        typeof(List<SerializableDrawingSurfaceModel>),
        typeof(List<SerializableTaggable>),
        typeof(SerializableDrawingSurfaceModel),
        typeof(SerializableDisplayAttribute),
        typeof(SerializableStencil),
        typeof(SerializableStencilEllipse),
        typeof(SerializableStencilParallelLines),
        typeof(SerializableStencilRectangle),
        typeof(SerializableStencilAnnotation),
        typeof(SerializableBorder),
        typeof(SerializableLine),
        typeof(SerializableHeaderDisplayAttribute),
        typeof(SerializableBooleanDisplayAttribute),
        typeof(SerializableStringDisplayAttribute),
        typeof(SerializableEnumerableDisplayAttribute),
        typeof(SerializableStaticBooleanDisplayAttribute),
        typeof(SerializableStaticEnumerableDisplayAttribute),
        typeof(SerializableStaticStringDisplayAttribute),
        typeof(SerializableListDisplayAttribute),
        typeof(SerializableStaticListDisplayAttribute),
        typeof(SerializableCustomStringDisplayAttribute),
        typeof(SerializableConnector),
        typeof(SerializableBorderBoundary),
        typeof(SerializableLineBoundary),
        typeof(SerializableTaggable),
        typeof(SerializableThreatMetaData),
        typeof(SerializableThreatMetaDatum),
        typeof(SerializableValidation),
        typeof(SerializableMetaInformation),
        typeof(SerializableThreat),
        typeof(List<string>),
        typeof(SerializableExtendable),
        typeof(SerializableProfile),
    ];

    // Preserve all model types so the trimmer/AOT compiler does not remove them.
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableModelData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableDrawingSurfaceModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableTaggable))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableBorder))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencil))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencilEllipse))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencilParallelLines))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencilRectangle))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencilAnnotation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableBorderBoundary))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableLine))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableConnector))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableLineBoundary))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableHeaderDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableBooleanDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStaticBooleanDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStringDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStaticStringDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableCustomStringDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableEnumerableDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStaticEnumerableDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableListDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStaticListDisplayAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableMetaInformation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableNote))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableThreat))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableValidation))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableKnowledgeBase))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableElementType))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableKnowledgeBaseAttribute))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableManifest))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableThreatCategory))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableThreatType))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableThreatMetaData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableThreatMetaDatum))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableExtendable))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableGenerationFilters))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableStencilConstraint))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableProfile))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableAttributeValues))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableAvailableToBaseModels))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(SerializableKbVersion))]
    private static DataContractSerializer CreateSerializer()
    {
        return new DataContractSerializer(typeof(SerializableModelData), KnownTypes);
    }

    /// <summary>
    /// Deserializes a .tm7 XML stream into a <see cref="SerializableModelData"/> instance.
    /// </summary>
    /// <param name="stream">A readable stream containing .tm7 XML data.</param>
    /// <returns>The deserialized threat model.</returns>
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "All serialized types are preserved via DynamicDependency attributes and TrimmerRoots.xml")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "All serialized types are preserved via DynamicDependency attributes and TrimmerRoots.xml")]
    public static SerializableModelData Deserialize(Stream stream)
    {
        var serializer = CreateSerializer();
        return (SerializableModelData)serializer.ReadObject(stream)!;
    }

    /// <summary>
    /// Serializes a <see cref="SerializableModelData"/> instance to .tm7 XML format.
    /// </summary>
    /// <param name="stream">A writable stream to receive the XML output.</param>
    /// <param name="model">The threat model to serialize.</param>
    [UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
        Justification = "All serialized types are preserved via DynamicDependency attributes and TrimmerRoots.xml")]
    [UnconditionalSuppressMessage("Trimming", "IL2026:RequiresUnreferencedCode",
        Justification = "All serialized types are preserved via DynamicDependency attributes and TrimmerRoots.xml")]
    public static void Serialize(Stream stream, SerializableModelData model)
    {
        var serializer = CreateSerializer();
        serializer.WriteObject(stream, model);
    }
}
