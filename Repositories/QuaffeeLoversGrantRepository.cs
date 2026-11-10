using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class QuaffeeLoversGrantRepository
    {
        public static bool TableSeen()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT CASE WHEN OBJECT_ID(N'dbo.QuaffeeLoversGrantTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        public QuaffeeLoversGrant GetByContactId(int contactId)
        {
            if (!TableSeen() || contactId <= 0)
                return null;
            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(
                "SELECT * FROM dbo.QuaffeeLoversGrantTbl WHERE ContactID = @Id",
                new List<DBParameter> { P("@Id", contactId, DbType.Int32) }))
            {
                if (r != null && r.Read())
                    return Map(r);
            }
            return null;
        }

        public void Save(QuaffeeLoversGrant grant)
        {
            if (!TableSeen() || grant == null || grant.ContactID <= 0)
                return;

            if (grant.GrantID > 0)
            {
                const string upd = @"
UPDATE dbo.QuaffeeLoversGrantTbl SET
    WooCustomerId = @WooCustomerId,
    MatchEmail = @MatchEmail,
    WooOrderCount = @WooOrderCount,
    WooRoleAtGrant = @WooRoleAtGrant,
    AlreadyMemberOnWoo = @AlreadyMemberOnWoo,
    RoleGrantedUtc = @RoleGrantedUtc,
    WelcomeEmailSentUtc = @WelcomeEmailSentUtc,
    InviteSentUtc = @InviteSentUtc,
    InviteSentBy = @InviteSentBy,
    DismissedUtc = @DismissedUtc,
    DismissedBy = @DismissedBy,
    Notes = @Notes
WHERE GrantID = @GrantID";
                using (var db = new TrackerSQLDb())
                    db.ExecuteNonQuery(upd, Params(grant, includeId: true));
                return;
            }

            const string ins = @"
INSERT INTO dbo.QuaffeeLoversGrantTbl (
    ContactID, WooCustomerId, MatchEmail, WooOrderCount, WooRoleAtGrant, AlreadyMemberOnWoo,
    RoleGrantedUtc, WelcomeEmailSentUtc, RecordedBy, InviteSentUtc, InviteSentBy, DismissedUtc, DismissedBy, Notes)
VALUES (
    @ContactID, @WooCustomerId, @MatchEmail, @WooOrderCount, @WooRoleAtGrant, @AlreadyMemberOnWoo,
    @RoleGrantedUtc, @WelcomeEmailSentUtc, @RecordedBy, @InviteSentUtc, @InviteSentBy, @DismissedUtc, @DismissedBy, @Notes)";
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(ins, Params(grant, includeId: false));
        }

        public List<QuaffeeLoversInviteRow> ListPendingInvites(int threshold, int maxRows = 500)
        {
            var list = new List<QuaffeeLoversInviteRow>();
            if (!TableSeen())
                return list;

            string sql = @"
SELECT TOP (" + maxRows.ToString(System.Globalization.CultureInfo.InvariantCulture) + @")
    c.ContactID,
    c.CompanyName,
    COALESCE(NULLIF(LTRIM(RTRIM(c.EmailAddress)), N''), NULLIF(LTRIM(RTRIM(c.AltEmailAddress)), N'')) AS Email,
    cnt.WooOrderCount,
    g.AlreadyMemberOnWoo,
    g.WooRoleAtGrant,
    g.WelcomeEmailSentUtc,
    g.DismissedUtc,
    g.Notes,
    wc.QuaffeeLoversMember
FROM (
    SELECT o.ContactID, COUNT(DISTINCT w.OrderID) AS WooOrderCount
    FROM dbo.WooOrderInfoTbl w
    INNER JOIN dbo.OrdersTbl o ON o.OrderID = w.OrderID
    WHERE o.ContactID > 0 AND o.ContactID <> @Sundry
    GROUP BY o.ContactID
    HAVING COUNT(DISTINCT w.OrderID) >= @Threshold
) cnt
INNER JOIN dbo.ContactsTbl c ON c.ContactID = cnt.ContactID
LEFT JOIN dbo.QuaffeeLoversGrantTbl g ON g.ContactID = c.ContactID
LEFT JOIN dbo.WooContactInfoTbl wc ON wc.ContactID = c.ContactID
WHERE (g.DismissedUtc IS NULL)
ORDER BY cnt.WooOrderCount DESC, c.CompanyName";

            var ps = new List<DBParameter>
            {
                P("@Threshold", threshold, DbType.Int32),
                P("@Sundry", (int)SystemConstants.CustomerConstants.SundryCustomerID, DbType.Int32)
            };

            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader(sql, ps))
            {
                while (r != null && r.Read())
                {
                    list.Add(new QuaffeeLoversInviteRow
                    {
                        ContactID = Convert.ToInt32(r["ContactID"]),
                        CompanyName = r["CompanyName"] == DBNull.Value ? string.Empty : r["CompanyName"].ToString(),
                        Email = r["Email"] == DBNull.Value ? string.Empty : r["Email"].ToString(),
                        WooOrderCount = Convert.ToInt32(r["WooOrderCount"]),
                        AlreadyMemberOnWoo = r["AlreadyMemberOnWoo"] != DBNull.Value && Convert.ToBoolean(r["AlreadyMemberOnWoo"]),
                        WooRoleOnShop = r["WooRoleAtGrant"] == DBNull.Value ? null : r["WooRoleAtGrant"].ToString(),
                        WelcomeSent = r["WelcomeEmailSentUtc"] != DBNull.Value,
                        Dismissed = r["DismissedUtc"] != DBNull.Value,
                        Notes = r["Notes"] == DBNull.Value ? null : r["Notes"].ToString()
                    });
                }
            }
            return list;
        }

        private static QuaffeeLoversGrant Map(IDataRecord r) => new QuaffeeLoversGrant
        {
            GrantID = Convert.ToInt32(r["GrantID"]),
            ContactID = Convert.ToInt64(r["ContactID"]),
            WooCustomerId = r["WooCustomerId"] == DBNull.Value ? (long?)null : Convert.ToInt64(r["WooCustomerId"]),
            MatchEmail = r["MatchEmail"] as string,
            WooOrderCount = Convert.ToInt32(r["WooOrderCount"]),
            WooRoleAtGrant = r["WooRoleAtGrant"] as string,
            AlreadyMemberOnWoo = r["AlreadyMemberOnWoo"] != DBNull.Value && Convert.ToBoolean(r["AlreadyMemberOnWoo"]),
            RoleGrantedUtc = r["RoleGrantedUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["RoleGrantedUtc"]),
            WelcomeEmailSentUtc = r["WelcomeEmailSentUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["WelcomeEmailSentUtc"]),
            RecordedUtc = Convert.ToDateTime(r["RecordedUtc"]),
            RecordedBy = r["RecordedBy"] as string,
            InviteSentUtc = r["InviteSentUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["InviteSentUtc"]),
            InviteSentBy = r["InviteSentBy"] as string,
            DismissedUtc = r["DismissedUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["DismissedUtc"]),
            DismissedBy = r["DismissedBy"] as string,
            Notes = r["Notes"] as string
        };

        private static List<DBParameter> Params(QuaffeeLoversGrant g, bool includeId)
        {
            var ps = new List<DBParameter>
            {
                P("@ContactID", g.ContactID, DbType.Int64),
                P("@WooCustomerId", g.WooCustomerId.HasValue && g.WooCustomerId.Value > 0 ? (object)g.WooCustomerId.Value : DBNull.Value, DbType.Int64),
                P("@MatchEmail", (object)g.MatchEmail ?? DBNull.Value, DbType.String),
                P("@WooOrderCount", g.WooOrderCount, DbType.Int32),
                P("@WooRoleAtGrant", (object)g.WooRoleAtGrant ?? DBNull.Value, DbType.String),
                P("@AlreadyMemberOnWoo", g.AlreadyMemberOnWoo, DbType.Boolean),
                P("@RoleGrantedUtc", (object)g.RoleGrantedUtc ?? DBNull.Value, DbType.DateTime2),
                P("@WelcomeEmailSentUtc", (object)g.WelcomeEmailSentUtc ?? DBNull.Value, DbType.DateTime2),
                P("@RecordedBy", (object)g.RecordedBy ?? DBNull.Value, DbType.String),
                P("@InviteSentUtc", (object)g.InviteSentUtc ?? DBNull.Value, DbType.DateTime2),
                P("@InviteSentBy", (object)g.InviteSentBy ?? DBNull.Value, DbType.String),
                P("@DismissedUtc", (object)g.DismissedUtc ?? DBNull.Value, DbType.DateTime2),
                P("@DismissedBy", (object)g.DismissedBy ?? DBNull.Value, DbType.String),
                P("@Notes", (object)g.Notes ?? DBNull.Value, DbType.String)
            };
            if (includeId)
                ps.Add(P("@GrantID", g.GrantID, DbType.Int32));
            return ps;
        }

        private static DBParameter P(string name, object value, DbType type) =>
            new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
    }
}
