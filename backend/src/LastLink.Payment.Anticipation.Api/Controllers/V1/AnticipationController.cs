using LastLink.Payment.Anticipation.Application.DTOs;
using LastLink.Payment.Anticipation.Application.Interfaces;
using LastLink.Payment.Anticipation.Domain.Enums;
using LastLink.Payment.Anticipation.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LastLink.Payment.Anticipation.Api.Controllers.V1
{
    /// <summary>
    /// Exposes REST endpoints for managing anticipation requests.
    /// This controller serves as the application’s external entry point,
    /// delegating all business workflows to the Application Layer.
    /// </summary>
    [ApiController]
    [Route("api/v1/anticipations")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Anticipations")]
    public sealed class AnticipationController : ControllerBase
    {
        private readonly IAnticipationService _service;

        public AnticipationController(IAnticipationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Creates a new anticipation request for a content creator.
        /// </summary>
        /// <remarks>
        /// This operation registers a new anticipation request, applying all
        /// domain-driven business rules before persistence.
        ///
        /// **Business rules enforced:**
        /// - The creator may have **only one pending** anticipation request.
        /// - Minimum allowed amount is **R$ 100,00**.
        /// - A standard fee of 5% is automatically applied.
        ///
        /// **Example Request Payload:**
        /// json
        /// {
        ///   "creatorId": "d9bd3083-03df-4e9a-a65c-5b1fbf0f681e",
        ///   "grossAmount": 150,
        ///   "createdAt": "2025-12-06T22:21:48.376Z"
        /// }
        /// 
        ///
        /// Returns: the complete anticipation record wrapped in a standard API envelope.
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAnticipationRequestDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Retrieves all anticipation requests associated with a specific creator.
        /// </summary>
        /// <remarks>
        /// Provides a historical list of all anticipation attempts submitted by a creator.
        /// This includes:
        ///
        /// - Pending requests  
        /// - Approved requests  
        /// - Rejected requests  
        ///
        /// Results are returned in descending order of creation date.
        ///
        /// **Example usage:**
        /// GET /api/v1/anticipations?creatorId=3fa85f64-5717-4562-b3fc-2c963f66afa6
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByCreator([FromQuery] Guid creatorId)
        {
            var result = await _service.GetByCreatorAsync(creatorId);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Updates the status of an anticipation request (approve or reject).
        /// </summary>
        /// <remarks>
        /// The allowed transitions are:
        ///
        /// - PENDING → APPROVED  
        /// - PENDING → REJECTED  
        ///
        /// The domain layer enforces:
        /// - Only pending requests may be updated  
        /// - A decision timestamp is automatically recorded  
        ///
        /// **Example Payload:**
        /// json
        /// { "status": 1 } // Approved
        /// 
        ///
        /// Returns the updated anticipation record in a standard API response envelope.
        /// </remarks>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] ApproveRejectDto dto)
        {
            bool approve = dto.Status == AnticipationStatus.Approved;
            var result = await _service.UpdateStatusAsync(id, approve);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Approves a pending anticipation request.
        /// </summary>
        /// <remarks>
        /// This action executes a domain-level state transition:
        /// 
        /// **PENDING → APPROVED**
        /// 
        /// Business rules enforced:
        /// - Only requests in *Pending* status may be approved.
        /// - Approval automatically records a decision timestamp.
        /// - Idempotency: approving an already-approved request is *not* allowed.
        /// 
        /// **Example Request**
        /// POST /api/v1/anticipations/{id}/approve
        /// 
        /// **Example Successful Response**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": {
        ///     "id": "uuid",
        ///     "status": 1,
        ///     "decisionAt": "2025-12-09T14:55:00Z"
        ///   },
        ///   "error": null
        /// }
        /// ```
        /// </remarks>
        [HttpPost("{id}/approve")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _service.UpdateStatusAsync(id, approve: true);
            return Ok(ApiResponse<object>.Ok(result));
        }


        /// <summary>
        /// Rejects a pending anticipation request.
        /// </summary>
        /// <remarks>
        /// This action executes a domain-level state transition:
        /// 
        /// **PENDING → REJECTED**
        /// 
        /// Business rules enforced:
        /// - Only requests that are *Pending* may be rejected.
        /// - A decision timestamp is automatically generated.
        /// - A rejected request cannot transition to another state.
        /// 
        /// **Example Request**
        /// POST /api/v1/anticipations/{id}/reject
        /// 
        /// **Example Successful Response**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": {
        ///     "id": "uuid",
        ///     "status": 2,
        ///     "decisionAt": "2025-12-09T15:22:10Z"
        ///   },
        ///   "error": null
        /// }
        /// ```
        /// </remarks>
        [HttpPost("{id}/reject")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject(Guid id)
        {
            var result = await _service.UpdateStatusAsync(id, approve: false);
            return Ok(ApiResponse<object>.Ok(result));
        }


        /// <summary>
        /// Simulates the anticipation fee and net value calculation.
        /// </summary>
        /// <remarks>
        /// This endpoint performs a pure domain calculation without persisting data.
        /// Ideal for UI preview scenarios.
        ///
        /// Returns:
        /// - Gross amount  
        /// - Calculated fee  
        /// - Net amount  
        /// - Simulation timestamp  
        ///
        /// **Example usage:**
        /// GET /api/v1/anticipations/simulate?grossAmount=200
        /// </remarks>
        [HttpGet("simulate")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Simulate([FromQuery] decimal grossAmount)
        {
            var result = await _service.SimulateAsync(grossAmount);
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Deletes all anticipation requests for a given creator.
        /// </summary>
        /// <remarks>
        /// This operation is strictly intended for **automated E2E test cleanup**
        /// and should not be used in production workflows.
        /// </remarks>
        [HttpDelete("cleanup")]
        public async Task<IActionResult> Cleanup([FromQuery] Guid creatorId)
        {
            await _service.CleanupAsync(creatorId);
            return Ok(ApiResponse<object>.Ok("Cleanup completed."));
        }
    }
}
