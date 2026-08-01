namespace Animora.SharedKernel.Validation.Clients;

/// <summary>
/// The property surface a patient (animal) create/edit command implements, validated directly by
/// <see cref="PatientValidator"/> (CONV-18) instead of a copied input DTO.
/// </summary>
public interface IPatientInput
{
    /// <summary>
    /// The owning <c>Owner</c>'s id; a patient belongs to exactly one owner (DOM-03). Whether that
    /// owner actually exists is a persistence-boundary lookup, which this I/O-free validator
    /// deliberately does not perform (SH-05) — only "not the empty id" is checked here.
    /// </summary>
    Guid OwnerId { get; }

    /// <summary>The patient's name.</summary>
    string Name { get; }

    /// <summary>The patient's species, from <see cref="PatientValidator.AllowedSpecies"/>.</summary>
    string Species { get; }

    /// <summary>The patient's sex, from <see cref="PatientValidator.AllowedSexes"/>.</summary>
    string Sex { get; }
}
