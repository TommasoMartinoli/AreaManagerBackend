using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADLoginAPI.Models;
using ADLoginAPI.Data;
using System;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using ADLoginAPI.DTO;

namespace ADLoginAPI.Controllers
{
    public class ActivitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/send-email-report")]
        public IActionResult SendEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                using (var smtpClient = new System.Net.Mail.SmtpClient("smtp.office365.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential("mautente02@zucchetti365lab.it", "J&118135280410ak");
                    smtpClient.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;

                    var mailMessage = new System.Net.Mail.MailMessage
                    {
                        //From = new System.Net.Mail.MailAddress(emailRequest.EmailSender),
                        From = new System.Net.Mail.MailAddress("mautente02@zucchetti365lab.it"),
                        Subject = emailRequest.EmailObject,
                        Body = emailRequest.EmailBody,
                        IsBodyHtml = false
                    };

                    var recipients = emailRequest.EmailReceiver
                        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var recipient in recipients)
                    {
                        mailMessage.To.Add(recipient.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(emailRequest.EmailCC))
                    {
                        var ccRecipients = emailRequest.EmailCC
                            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var ccRecipient in ccRecipients)
                        {
                            mailMessage.CC.Add(ccRecipient.Trim());
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(emailRequest.EmailReplyTo))
                    {
                        mailMessage.ReplyToList.Add(new System.Net.Mail.MailAddress(emailRequest.EmailReplyTo.Trim()));
                    }

                    foreach (var attachment in emailRequest.Attachments)
                    {
                        var fileBytes = Convert.FromBase64String(attachment.Content);
                        var stream = new System.IO.MemoryStream(fileBytes);
                        mailMessage.Attachments.Add(new System.Net.Mail.Attachment(stream, attachment.Name));
                    }

                    smtpClient.Send(mailMessage);
                }

                return Ok(new { success = true, message = "Email inviata con successo." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("/api/upsert-tm-note")]
        public async Task<IActionResult> UpsertPrivateNote([FromBody] PrivateNoteDTO privateNoteDto)
        {
            try
            {
                var countQuery = @"
                SELECT COUNT(1) AS CountResult
                FROM PAI_EmployeesActivitiesPrivateNotes
                WHERE ActivityYear = @ActivityYear
                  AND Week = @Week
                  AND PICEmployee = @PICEmployee
                  AND IsTMNote = @IsTMNote
                  AND TeamRole = @TeamRole
                  AND PICWriter = @PICWriter";

                // Eseguiamo la query
                var result = await _context.CountResults
                    .FromSqlRaw(countQuery,
                        new SqlParameter("@ActivityYear", privateNoteDto.ActivityYear),
                        new SqlParameter("@Week", privateNoteDto.Week),
                        new SqlParameter("@PICEmployee", privateNoteDto.PICEmployee),
                        new SqlParameter("@IsTMNote", privateNoteDto.IsTMNote),
                        new SqlParameter("@TeamRole", privateNoteDto.TeamRole ?? (object)DBNull.Value),
                        new SqlParameter("@PICWriter", privateNoteDto.PICWriter))
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                int count = result?.CountResult ?? 0;

                if (count > 0)
                {
                    var updateQuery = @"
                        UPDATE PAI_EmployeesActivitiesPrivateNotes
                        SET Notes = @Notes,
                            TBModified = @TBModified
                        WHERE ActivityYear = @ActivityYear
                          AND Week = @Week
                          AND PICEmployee = @PICEmployee
                          AND IsTMNote = @IsTMNote
                          AND TeamRole = @TeamRole
                          AND PICWriter = @PICWriter";

                    await _context.Database.ExecuteSqlRawAsync(updateQuery,
                        new SqlParameter("@Notes", privateNoteDto.Notes ?? (object)DBNull.Value),
                        new SqlParameter("@TBModified", DateTime.Now),
                        new SqlParameter("@ActivityYear", privateNoteDto.ActivityYear),
                        new SqlParameter("@Week", privateNoteDto.Week),
                        new SqlParameter("@PICEmployee", privateNoteDto.PICEmployee),
                        new SqlParameter("@IsTMNote", privateNoteDto.IsTMNote),
                        new SqlParameter("@TeamRole", privateNoteDto.TeamRole ?? (object)DBNull.Value),
                        new SqlParameter("@PICWriter", privateNoteDto.PICWriter));
                }
                else
                {
                    var insertQuery = @"
                        INSERT INTO PAI_EmployeesActivitiesPrivateNotes
                        (ActivityYear, Week, PICEmployee, IsTMNote, Notes, TeamRole, PICWriter, TBCreated, TBModified)
                        VALUES
                        (@ActivityYear, @Week, @PICEmployee, @IsTMNote, @Notes, @TeamRole, @PICWriter, @TBCreated, @TBModified)";

                    await _context.Database.ExecuteSqlRawAsync(insertQuery,
                        new SqlParameter("@ActivityYear", privateNoteDto.ActivityYear),
                        new SqlParameter("@Week", privateNoteDto.Week),
                        new SqlParameter("@PICEmployee", privateNoteDto.PICEmployee),
                        new SqlParameter("@IsTMNote", privateNoteDto.IsTMNote),
                        new SqlParameter("@Notes", privateNoteDto.Notes ?? (object)DBNull.Value),
                        new SqlParameter("@TeamRole", privateNoteDto.TeamRole ?? (object)DBNull.Value),
                        new SqlParameter("@PICWriter", privateNoteDto.PICWriter),
                        new SqlParameter("@TBCreated", DateTime.Now),
                        new SqlParameter("@TBModified", DateTime.Now));
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("/api/db-check")]
        public IActionResult DbCheck()
        {
            var defaultConnection = AppConfig.Configuration.GetConnectionString("DefaultConnection");
            Boolean isTest = defaultConnection.Contains("db-test", StringComparison.OrdinalIgnoreCase);

            return Ok(new
            {
                isTest
            });
        }

        [HttpGet("/api/all-data")]
        public IActionResult GetAllData([FromQuery] int teamRole, [FromQuery] string teamRoleDescription, [FromQuery] int? week, [FromQuery] int? year, [FromQuery] string myPICEmployee)
        {
            try
            {
                var employeeQuery = @"
                    SELECT PICEmployee,HoursPerWeek,Name,Surname
                    FROM PAI_EmployeesMaster
                    WHERE disabled = '0' 
                    AND (CustomerCode = '0110G081' OR CustomerCode = 'ZUCCH_WBO') 
                    AND Division<> 39256064
                    AND(dbo.BelongToRoleID('Microarea SpA', PAI_EmployeesMaster.CRMUserName, @teamRole) = 1  
                    OR exists(SELECT * FROM[dbo].[PAI_FnGetSubRolesByRoleID]('Microarea SpA', @teamRole) X
                    WHERE dbo.BelongToRoleID('Microarea SpA', PAI_EmployeesMaster.CRMUserName, X.RoleID) = 1))";

                var employees = _context.PAI_EmployeesMasterForRoles
                    .FromSqlRaw(employeeQuery, new SqlParameter("@teamRole", teamRole))
                    .Select(e => new EmployeeDTO
                    {
                        PICEmployee = e.PICEmployee,
                        Name = e.Name,
                        Surname = e.Surname,
                        HoursPerWeek = e.HoursPerWeek
                    })
                    .OrderBy(e => e.Surname)  
                    .ThenBy(e => e.Name)
                    .ToList();

                if (employees.Count() < 1)
                {
                    return Ok(new
                    {
                        employees
                    });
                }

                var picEmployees = employees
                    .Select(e => e.PICEmployee)
                    .ToList();

                var activityTypeMap = new Dictionary<int, string> {
                    { 36044800, "Generica" },
                    { 36044801, "Sviluppo SW" },
                    { 36044802, "Manutenzione SW" },
                    { 36044803, "Analisi SW" },
                    { 36044804, "Testing SW" },
                    { 36044805, "Documentazione" },
                    { 36044806, "Produzione Release" },
                    { 36044807, "Assistenza" },
                    { 36044808, "Formazione" },
                    { 36044809, "Coordinamento" },
                    { 36044810, "Riunione" },
                    { 36044811, "Marketing" },
                    { 36044812, "Commerciale" },
                    { 36044813, "Amministrativa" },
                    { 36044814, "Customer Care" },
                    { 36044815, "Gestione IT" },
                    { 36044816, "Assenza" }
                };

                var activitiesRowsQuery = @"
                    SELECT r.ActivityYear, r.Week, r.PICEmployee, r.RecordingDate, r.RowID, 
                           r.TaskCode, r.ActivityTime, r.Description, r.LinkDetail, r.ProductCode, 
                           r.ReleaseNr, r.CompanyCode, j.ActivityType 
                    FROM PAI_EmployeesActivitiesRowsFinal r
                    JOIN PAI_JobActivityCodes j ON r.TaskCode = j.TaskCode
                    WHERE r.PICEmployee IN ({0})
                    AND (@year IS NULL OR r.ActivityYear = @year)
                    AND (@week IS NULL OR r.Week = @week)";

                var activitiesRows = _context.PAI_EmployeesActivitiesRowsFinal
                    .FromSqlRaw(string.Format(activitiesRowsQuery, string.Join(",", picEmployees.Select(e => $"'{e}'"))),
                        new SqlParameter("@year", year ?? (object)DBNull.Value),
                        new SqlParameter("@week", week ?? (object)DBNull.Value))
                    .Select(r => new ActivityRowDTO
                    {
                        ActivityYear = r.ActivityYear,
                        Week = r.Week,
                        PICEmployee = r.PICEmployee ?? string.Empty,
                        RecordingDate = r.RecordingDate,
                        RowID = r.RowID,
                        ActivityTime = r.ActivityTime,
                        Description = r.Description ?? string.Empty,
                        LinkDetail = r.LinkDetail,
                        ProductCode = r.ProductCode ?? string.Empty,
                        ReleaseNr = r.ReleaseNr ?? string.Empty,
                        CompanyCode = r.CompanyCode ?? string.Empty,
                        TaskCode = r.ActivityType.ToString(),
                        TaskCodeDescription = string.Empty
                    })
                    .ToList();

                activitiesRows.ForEach(row =>
                {
                    if (int.TryParse(row.TaskCode, out var storedKey) && activityTypeMap.ContainsKey(storedKey)) {
                        row.TaskCodeDescription = activityTypeMap[storedKey];
                    }
                    else {
                        row.TaskCodeDescription = "Generica";
                    }
                });

                var activitiesQuery = @"
                    SELECT ActivityYear, Week, PICEmployee, PrevNotes, Notes
                    FROM PAI_EmployeesActivities
                    WHERE PICEmployee IN ({0})
                    AND (@year IS NULL OR ActivityYear = @year)
                    AND (@week IS NULL OR Week = @week)";
                
                var activities = _context.PAI_EmployeesActivities
                    .FromSqlRaw(string.Format(activitiesQuery, string.Join(",", picEmployees.Select(e => $"'{e}'"))),
                        new SqlParameter("@year", year ?? (object)DBNull.Value),
                        new SqlParameter("@week", week ?? (object)DBNull.Value))
                    .Select(a => new ActivityDTO
                    {
                        ActivityYear = a.ActivityYear,
                        Week = a.Week,
                        PICEmployee = a.PICEmployee,
                        PrevNotes = a.PrevNotes,
                        Notes = a.Notes
                    })
                    .ToList();

                var privateNotesQuery = @"
                    SELECT ActivityYear, Week, PICEmployee, IsTMNote, Notes, TeamRole, PICWriter
                    FROM PAI_EmployeesActivitiesPrivateNotes
                    WHERE (@year IS NULL OR ActivityYear = @year)
                    AND (@week IS NULL OR Week = @week)
                    AND (@teamRoleDescription IS NULL OR TeamRole LIKE @teamRoleDescription)";

                var privateNotes = _context.PAI_EmployeesActivitiesPrivateNotes
                    .FromSqlRaw(privateNotesQuery,
                        new SqlParameter("@year", year ?? (object)DBNull.Value),
                        new SqlParameter("@week", week ?? (object)DBNull.Value),
                        new SqlParameter("@teamRoleDescription", string.IsNullOrEmpty(teamRoleDescription) ? (object)DBNull.Value : $"%{teamRoleDescription}%"))
                    .Select(n => new PrivateNoteDTO
                    {
                        ActivityYear = n.ActivityYear,
                        Week = n.Week,
                        PICEmployee = n.PICEmployee ?? string.Empty,
                        IsTMNote = n.IsTMNote,
                        Notes = n.Notes,
                        TeamRole = n.TeamRole ?? string.Empty,
                        PICWriter = n.PICWriter ?? string.Empty
                    })
                    .ToList();

                return Ok(new
                {
                    employees,
                    activitiesRows,
                    activities,
                    privateNotes
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
    }
}