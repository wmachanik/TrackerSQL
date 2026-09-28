using System;
using System.Collections.Generic;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Repairs for the driver app. Saves follow the web Repair Detail rules: ZZName needs a contact name,
    /// a related order (repair-check line) is created when missing, order notes are kept in sync, and the
    /// contact is emailed when the status changes.
    /// </summary>
    public class MobileRepairManager
    {
        private const string SyncKindRepair = "repair";

        private readonly MobileApiRepository _repo = new MobileApiRepository();
        private readonly RepairManager _repairs = new RepairManager();

        public List<MobileRepair> List(bool openOnly, int? contactId, DateTime? since, int max)
        {
            return _repo.ListRepairs(openOnly, contactId, since, max <= 0 ? 100 : max);
        }

        public MobileRepair Get(int repairId)
        {
            return _repo.GetRepair(repairId);
        }

        public MobileRepairSaveResult Create(MobileRepairSave save, MobileTokenUser user)
        {
            MobileApiSchemaInstaller.EnsureReady();
            if (save == null)
                return Fail("No repair was sent.");

            string clientRef = (save.ClientRef ?? string.Empty).Trim();
            if (clientRef.Length > 64)
                return Fail("clientRef can be up to 64 characters.");

            if (clientRef.Length > 0)
            {
                int? existingId = _repo.GetSyncEntityId(clientRef, SyncKindRepair);
                if (existingId.HasValue)
                    return new MobileRepairSaveResult
                    {
                        Ok = true,
                        Duplicate = true,
                        Message = "Already created.",
                        Repair = _repo.GetRepair(existingId.Value)
                    };
            }

            if (!save.ContactId.HasValue || save.ContactId.Value <= 0)
                return Fail("contactId is required.");
            if (new ContactsRepository().GetById(save.ContactId.Value) == null)
                return Fail("Contact " + save.ContactId.Value + " was not found.");

            int repairId = _repairs.CreateRepairForContact(save.ContactId.Value);
            if (repairId <= 0)
                return Fail("The repair could not be created.");

            if (clientRef.Length > 0)
                _repo.InsertSyncRef(clientRef, SyncKindRepair, repairId, user?.UserName);

            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                "Repair " + repairId + " | Contact=" + save.ContactId.Value + " | Repair created (mobile)", user?.UserName);

            var result = ApplyAndSave(repairId, save, user, isNew: true);
            if (!result.Ok)
                result.Message = "Repair " + repairId + " was created, but " + result.Message;
            return result;
        }

        public MobileRepairSaveResult Update(int repairId, MobileRepairSave save, MobileTokenUser user)
        {
            if (save == null)
                return Fail("No changes were sent.");
            if (_repairs.GetRepairFormDataById(repairId) == null)
                return Fail("Repair " + repairId + " was not found.");
            return ApplyAndSave(repairId, save, user, isNew: false);
        }

        private MobileRepairSaveResult ApplyAndSave(int repairId, MobileRepairSave save, MobileTokenUser user, bool isNew)
        {
            var repair = _repairs.GetRepairFormDataById(repairId);
            if (repair == null)
                return Fail("Repair " + repairId + " was not found.");

            int previousStatusId = repair.RepairStatusID;

            if (save.ContactName != null) repair.ContactName = save.ContactName.Trim();
            if (save.ContactEmail != null) repair.ContactEmail = save.ContactEmail.Trim();
            if (save.JobCard != null) repair.JobCardNumber = save.JobCard.Trim();
            if (save.EquipTypeId.HasValue) repair.MachineTypeID = save.EquipTypeId.Value;
            if (save.Serial != null) repair.MachineSerialNumber = save.Serial.Trim();
            if (save.SwopOutId.HasValue) repair.SwopOutMachineID = save.SwopOutId.Value;
            if (save.ConditionId.HasValue) repair.MachineConditionID = save.ConditionId.Value;
            if (save.TakenFrother.HasValue) repair.TakenFrother = save.TakenFrother.Value;
            if (save.TakenBeanLid.HasValue) repair.TakenBeanLid = save.TakenBeanLid.Value;
            if (save.TakenWaterLid.HasValue) repair.TakenWaterLid = save.TakenWaterLid.Value;
            if (save.BrokenFrother.HasValue) repair.BrokenFrother = save.BrokenFrother.Value;
            if (save.BrokenBeanLid.HasValue) repair.BrokenBeanLid = save.BrokenBeanLid.Value;
            if (save.BrokenWaterLid.HasValue) repair.BrokenWaterLid = save.BrokenWaterLid.Value;
            if (save.FaultId.HasValue) repair.RepairFaultID = save.FaultId.Value;
            if (save.FaultDesc != null) repair.RepairFaultDesc = save.FaultDesc.Trim();
            if (save.StatusId.HasValue) repair.RepairStatusID = save.StatusId.Value;
            if (save.Notes != null) repair.Notes = save.Notes;

            string sundryError = _repairs.ValidateSundryContactName(repair);
            if (!string.IsNullOrEmpty(sundryError))
                return Fail(sundryError, repairId);

            string orderNote = _repairs.EnsureRelatedOrder(repair);

            bool statusChanged = repair.RepairStatusID != previousStatusId;
            bool notify = save.Notify ?? (statusChanged || isNew);
            // Every save moves LastStatusChange so "since" lists pick the repair up.
            repair.LastStatusChange = TimeZoneUtils.Now();
            string message;
            if (notify)
            {
                // Saves and emails the contact, as Repair Detail does.
                string result = _repairs.HandleStatusChange(repair);
                string updateError = MessageProvider.Get(MessageKeys.Repairs.ErrorUpdating);
                if (!string.IsNullOrWhiteSpace(result) && string.Equals(result.Trim(), (updateError ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase))
                    return Fail(result, repairId);
                message = string.IsNullOrWhiteSpace(result) ? "Saved; the contact was emailed." : "Saved, but the email failed: " + result;
            }
            else
            {
                string result = _repairs.UpdateRepair(repair, repairId);
                if (!string.IsNullOrWhiteSpace(result))
                    return Fail(result, repairId);
                message = "Saved.";
            }

            if (repair.RelatedOrderLineID > 0)
                _repairs.SyncRelatedOrderNotes(repair);

            if (!string.IsNullOrWhiteSpace(orderNote))
                message += " " + orderNote;

            AppLogger.WriteLog(SystemConstants.LogTypes.Repairs,
                "Repair " + repairId + " | Contact=" + repair.CustomerID + " | "
                + (statusChanged ? "Status changed from=" + previousStatusId + " to=" + repair.RepairStatusID : "Repair saved")
                + " (mobile)", user?.UserName);

            return new MobileRepairSaveResult { Ok = true, Message = message, Repair = _repo.GetRepair(repairId) };
        }

        private MobileRepairSaveResult Fail(string message, int repairId = 0)
        {
            return new MobileRepairSaveResult
            {
                Ok = false,
                Message = message,
                Repair = repairId > 0 ? _repo.GetRepair(repairId) : null
            };
        }
    }
}
