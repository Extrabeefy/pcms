using PCMSApi.Models;

namespace PCMSApi.Services
{
    /// <summary>
    /// Interface for patient service operations.
    /// </summary>
    public interface IPatientService
    {
        /// <summary>
        /// Retrieves a paginated list of patients.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The number of patients per page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of PatientDto objects.</returns>
        Task<List<PatientDto>> GetAllPatientsAsync(int page, int pageSize, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a patient by their unique identifier.
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A PatientDto object if found, otherwise null.</returns>
        Task<PatientDto?> GetPatientByIdAsync(Guid patientId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new patient.
        /// </summary>
        /// <param name="dto">The patient data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created PatientDto object.</returns>
        Task<PatientDto> CreatePatientAsync(PatientDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new patient with associated files.
        /// </summary>
        /// <param name="dto">The patient data transfer object.</param>
        /// <param name="files">The list of files to upload.</param>
        /// <param name="documentTypes">The list of document types corresponding to the files.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The created PatientDto object.</returns>
        Task<PatientDto> CreatePatientWithFilesAsync(PatientDto dto, List<IFormFile> files, List<string> documentTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing patient.
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <param name="dto">The patient data transfer object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        Task<bool> UpdatePatientAsync(Guid patientId, PatientDto dto, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing patient with associated files.
        /// </summary>
        /// <param name="patientUid">The unique identifier of the patient.</param>
        /// <param name="dto">The patient data transfer object.</param>
        /// <param name="files">The list of files to upload.</param>
        /// <param name="documentTypes">The list of document types corresponding to the files.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the update was successful, otherwise false.</returns>
        Task<bool> UpdatePatientWithFilesAsync(Guid patientUid, PatientDto dto, List<IFormFile> files, List<string> documentTypes, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a patient by their unique identifier.
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        Task<bool> DeletePatientAsync(Guid patientId, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an attachment by its unique identifier.
        /// </summary>
        /// <param name="patientUid">The unique identifier of the patient.</param>
        /// <param name="attachmentId">The unique identifier of the attachment.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the deletion was successful, otherwise false.</returns>
        Task<bool> DeleteAttachmentAsync(Guid patientUid, Guid attachmentId, CancellationToken cancellationToken);
    }
}