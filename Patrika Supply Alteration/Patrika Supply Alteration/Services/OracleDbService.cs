using DCRSupplyApp.Models;
using Oracle.ManagedDataAccess.Client;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace DCRSupplyApp.Services;

public class OracleDbService
{
    private readonly string _connectionString;

    public OracleDbService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("OracleDb")!;
    }

    private OracleConnection GetConnection() => new OracleConnection(_connectionString);

    // QUERY 1: Get roles
    public async Task<List<RoleViewModel>> GetRolesAsync()
    {
        var roles = new List<RoleViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new OracleCommand("SELECT CODE, NAME FROM CIR_PLI_HIERARCHY", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            roles.Add(new RoleViewModel
            {
                Code = reader["CODE"]?.ToString(),
                Name = reader["NAME"]?.ToString()
            });
        }
        return roles;
    }

    // Auto-detect role login
    //public async Task<UserSessionModel?> LoginAutoDetectAsync(string username, string password)
    //{
    //    // First try hierarchy-based login (SE/HO roles)
    //    var hierarchyUser = await LoginWithHierarchyAsync(username, password);
    //    if (hierarchyUser != null)
    //        return hierarchyUser;

    //    // Then try Zonal Head
    //    var zhUser = await CheckZonalHead(username, password);
    //    if (zhUser != null)
    //        return zhUser;

    //    return null;
    //}

    //private async Task<UserSessionModel?> LoginWithHierarchyAsync(string username, string password)
    //{
    //    using var conn = GetConnection();
    //    await conn.OpenAsync();
    //    var sql = @"SELECT L.""userid"" AS USERID, L.HR_CODE, L.COM_CODE, L.STATUS,
    //                E.EMP_CODE, E.NAME, E.DESIG, E.BRAN_CODE,
    //                E.MOBILE, E.EMAIL, E.ZONE, E.REPORT_TO,
    //                CPH.NAME AS ROLE_NAME, CPHM.HIERARCHY_CODE
    //                FROM LOGIN L
    //                LEFT JOIN HR_EMP_MST E ON E.EMP_CODE = L.HR_CODE
    //                LEFT JOIN CIR_PLI_HIERARCHY_MAST CPHM ON CPHM.EMPLOYEE_CODE = L.HR_CODE
    //                LEFT JOIN CIR_PLI_HIERARCHY CPH ON CPH.CODE = CPHM.HIERARCHY_CODE
    //                WHERE L.""username"" = :USERNAME
    //                AND L.SUPPLY_ALTERATION_PASSWORD = :PASSWORD
    //                AND L.STATUS = 'A'
    //                AND CPHM.HIERARCHY_CODE IS NOT NULL";
    //    using var cmd = new OracleCommand(sql, conn);
    //    cmd.Parameters.Add(new OracleParameter("USERNAME", username));
    //    cmd.Parameters.Add(new OracleParameter("PASSWORD", password));
    //    using var reader = await cmd.ExecuteReaderAsync();
    //    if (await reader.ReadAsync())
    //    {
    //        var hierarchyCode = reader["HIERARCHY_CODE"]?.ToString();
    //        return new UserSessionModel
    //        {
    //            UserId = reader["USERID"]?.ToString(),
    //            HrCode = reader["HR_CODE"]?.ToString(),
    //            ComCode = reader["COM_CODE"]?.ToString(),
    //            EmpCode = reader["EMP_CODE"]?.ToString(),
    //            EmpName = reader["NAME"]?.ToString(),
    //            Designation = reader["DESIG"]?.ToString(),
    //            BranchCode = reader["BRAN_CODE"]?.ToString(),
    //            Mobile = reader["MOBILE"]?.ToString(),
    //            Email = reader["EMAIL"]?.ToString(),
    //            Zone = reader["ZONE"]?.ToString(),
    //            ReportTo = reader["REPORT_TO"]?.ToString(),
    //            RoleName = reader["ROLE_NAME"]?.ToString(),
    //            HierarchyCode = hierarchyCode,
    //            SelectedRole = hierarchyCode
    //        };
    //    }
    //    return null;
    //}

    // QUERY 2: Login verification
    public async Task<UserSessionModel?> LoginAsync(string username, string password)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        bool LoginVerify = false;
        bool ExecutiveLogin = false;

        try
        {
            var (verified, firstLoginFlag) = await UserCheckOnLoginWithFlag(username, password);
            LoginVerify = verified;

            if(LoginVerify==false)
            {
                return null;
            }

            #region Check user does executive or not
            var execData = await CheckUserExecutive(username);
            if(execData!=null && !string.IsNullOrEmpty(execData.HrCode))
            {
                execData.FirstLoginFlag = firstLoginFlag;
                return execData;
            }
            #endregion Complete Executive 

            #region Check user does HO or not
            var HOData = await CheckUserHO(username);
            if (HOData != null && !string.IsNullOrEmpty(HOData.HrCode))
            {
                HOData.FirstLoginFlag = firstLoginFlag;
                return HOData;
            }
            #endregion Complete HO 

            // If not executive, check other roles
            var otherData = await OTHERLOGINUSER(username);
            if(otherData!=null && otherData.BranchDetails!=null && otherData.BranchDetails.Count()>0)
            {
                otherData.FirstLoginFlag = firstLoginFlag;
                return otherData;
            }

            return null;
        }
        catch (Exception ex)
        {

        }
        return null;
     
    }

    public async Task<UserSessionModel> OTHERLOGINUSER(string EMPLOYEECODE)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();

        try
        {
            var RoleDetails = new List<RoleDetails>();
            var branchDetails = new List<BranchDetail>();

            var sql1 = @"SELECT  JPCM.""Pub_cent_Code"" AS BranchCode, JPCM.""Pub_Cent_name"" AS BranchName ,CPH.CODE AS ROLEID ,CPH.NAME AS ROLENAME
                     FROM CIR_PLI_HIERARCHY_MAST CEM 
                     LEFT JOIN PUB_CENT_MAST JPCM ON JPCM.""Pub_cent_Code"" = CEM.UNIT_CODE
                     LEFT JOIN CIR_PLI_HIERARCHY CPH ON CPH.CODE = CEM.HIERARCHY_CODE
                     WHERE 
                        ISACTIVEFORPLI = 'Y' 
                       AND EMPLOYEE_CODE = :EmployeeCode";

            using (var cmd1 = new OracleCommand(sql1, conn))
            {
                cmd1.Parameters.Add(new OracleParameter("EmployeeCode", EMPLOYEECODE));

                using var reader1 = await cmd1.ExecuteReaderAsync();

                while (await reader1.ReadAsync())   
                {

                    branchDetails.Add(new BranchDetail
                    {
                        BranchCode = reader1["BranchCode"]?.ToString(),
                        BranchName = reader1["BranchName"]?.ToString()
                    });

                    RoleDetails.Add(new RoleDetails {
                    RoleId  = reader1["ROLEID"]?.ToString(),
                    RoleName = reader1["ROLENAME"]?.ToString()

                    });
                }
                if(RoleDetails!=null && RoleDetails.Count()>0 && RoleDetails.Any(x=>x.RoleId=="4"))
                {
                    var SqlUpdate = @"SELECT DISTINCT(CPHM.UNIT_CODE)AS BranchCode, JPCM.""Pub_Cent_name"" AS BranchName FROM CIR_PLI_HIERARCHY_MAPPING CPHM
                                 LEFT JOIN CIR_PLI_HIERARCHY_MAST HM ON CPHM.ZONAL_HEAD = HM.CODE
                                 LEFT JOIN PUB_CENT_MAST JPCM ON JPCM.""Pub_cent_Code"" = CPHM.UNIT_CODE
                                 WHERE HM.EMPLOYEE_CODE = :EmployeeCode";
                    using (var cmdupdate = new OracleCommand(SqlUpdate, conn))
                    {
                        cmdupdate.Parameters.Add(new OracleParameter("EmployeeCode", EMPLOYEECODE));

                        using var readerupdate = await cmdupdate.ExecuteReaderAsync();

                        while (await readerupdate.ReadAsync())
                        {

                            branchDetails.Add(new BranchDetail
                            {
                                BranchCode = readerupdate["BranchCode"]?.ToString(),
                                BranchName = readerupdate["BranchName"]?.ToString()
                            });

                          
                        }



                    }
                }

                // If no rows found — employee is not an active executive
                if (branchDetails!=null && branchDetails.Count == 0)
                {
                    return null;
                }
            }

            // ?? Second Query: Get user session details ??
            var sql2 = @"SELECT L.""userid"" AS USERID, L.HR_CODE, L.COM_CODE, L.STATUS,
                            E.EMP_CODE, E.NAME, E.DESIG, E.BRAN_CODE,
                            E.MOBILE, E.EMAIL, E.ZONE, E.REPORT_TO
                     FROM LOGIN L
                     LEFT JOIN HR_EMP_MST E ON E.EMP_CODE = L.HR_CODE
                     WHERE E.EMP_CODE = :EmployeeCode
                       AND L.STATUS = 'A'";

            using var cmd2 = new OracleCommand(sql2, conn);
            cmd2.Parameters.Add(new OracleParameter("EmployeeCode", EMPLOYEECODE));

            using var reader2 = await cmd2.ExecuteReaderAsync();
            if (await reader2.ReadAsync())
            {
                var data = new UserSessionModel
                {
                    UserId = reader2["USERID"]?.ToString(),
                    HrCode = reader2["HR_CODE"]?.ToString(),
                    ComCode = reader2["COM_CODE"]?.ToString(),
                    EmpCode = reader2["EMP_CODE"]?.ToString(),
                    EmpName = reader2["NAME"]?.ToString(),
                    Designation = reader2["DESIG"]?.ToString(),
                    BranchCode = reader2["BRAN_CODE"]?.ToString(),
                    Mobile = reader2["MOBILE"]?.ToString(),
                    Email = reader2["EMAIL"]?.ToString(),
                    Zone = reader2["ZONE"]?.ToString(),
                    ReportTo = reader2["REPORT_TO"]?.ToString(),
                    // ?? Carry forward values from first query ??
                    UnitCode = reader2["BRAN_CODE"]?.ToString(),
                    BranchDetails = branchDetails,
                    RoleDetails = RoleDetails,
                    RoleName = RoleDetails.FirstOrDefault()?.RoleName ?? ""

                };

                return data;
            }

            return null; // No matching login record found
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckUserExecutive: {ex.Message}");
            return null;
        }


        return null;
    }

    public async Task<bool> UserCheckOnLogin(string username, string password)
    {
        var (verified, _) = await UserCheckOnLoginWithFlag(username, password);
        return verified;
    }

    public async Task<(bool verified, bool firstLoginFlag)> UserCheckOnLoginWithFlag(string username, string password)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        bool LoginVerify = false;
        bool firstLogin = false;

        try
        {
            var sql = @"SELECT L.""userid"" AS USERID, L.HR_CODE, L.COM_CODE, L.STATUS,
                    E.EMP_CODE, E.NAME, E.DESIG, E.BRAN_CODE,
                    E.MOBILE, E.EMAIL, E.ZONE, E.REPORT_TO,
                    NVL(L.FIRST_LOGIN_FLAG, 0) AS FIRST_LOGIN_FLAG

                    FROM LOGIN L
                    LEFT JOIN HR_EMP_MST E ON E.EMP_CODE = L.HR_CODE                   
                    WHERE L.HR_CODE = :USERNAME
                    AND L.SUPPLY_ALTERATION_PASSWORD = :PASSWORD
                    AND L.STATUS = 'A'
                    ";
            using var cmd = new OracleCommand(sql, conn);
            cmd.Parameters.Add(new OracleParameter("USERNAME", username));
            cmd.Parameters.Add(new OracleParameter("PASSWORD", password));
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var data = new UserSessionModel
                {
                    UserId = reader["USERID"]?.ToString(),
                    HrCode = reader["HR_CODE"]?.ToString(),
                    ComCode = reader["COM_CODE"]?.ToString(),
                    EmpCode = reader["EMP_CODE"]?.ToString(),
                    EmpName = reader["NAME"]?.ToString(),
                    Designation = reader["DESIG"]?.ToString(),
                    BranchCode = reader["BRAN_CODE"]?.ToString(),
                    Mobile = reader["MOBILE"]?.ToString(),
                    Email = reader["EMAIL"]?.ToString(),
                    Zone = reader["ZONE"]?.ToString(),
                    ReportTo = reader["REPORT_TO"]?.ToString()

                };

                if(data!=null && data.UserId!=null)
                {
                    LoginVerify = true;
                }

                var flagValue = reader["FIRST_LOGIN_FLAG"] != DBNull.Value ? Convert.ToInt32(reader["FIRST_LOGIN_FLAG"]) : 0;
                firstLogin = flagValue == 0;
            }
        }



        catch (Exception ex)
        {

        }
        return (LoginVerify, firstLogin);
    }

    public async Task<bool> ChangePasswordAsync(string employeeCode, string newPassword)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                var sql = @"UPDATE LOGIN SET SUPPLY_ALTERATION_PASSWORD = :NEW_PASSWORD, FIRST_LOGIN_FLAG = 1 WHERE HR_CODE = :EMP_CODE AND STATUS = 'A'";
                using var cmd = new OracleCommand(sql, conn) { Transaction = txn };
                cmd.Parameters.Add(new OracleParameter("NEW_PASSWORD", newPassword));
                cmd.Parameters.Add(new OracleParameter("EMP_CODE", employeeCode));
                var rows = await cmd.ExecuteNonQueryAsync();
                txn.Commit();
                return rows > 0;
            }
            catch
            {
                txn.Rollback();
                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<UserSessionModel> CheckUserExecutive(string EmployeeCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        try
        {
            // ?? First Query: Get all branch details from executive master ??
            var RoleDetails = new List<RoleDetails>();

            var branchDetails = new List<BranchDetail>();

            var sql1 = @"SELECT  CEM.UNIT_CODE AS BranchCode, JPCM.""Pub_Cent_name"" AS BranchName
                     FROM CIR_EXECUTIVE_MAST CEM 
                     LEFT JOIN PUB_CENT_MAST JPCM ON JPCM.""Pub_cent_Code"" = CEM.UNIT_CODE
                     WHERE EXEC_DESIGNATION = 'EXEC' 
                       AND ISACTIVEFORPLI = 'Y' 
                       AND HR_CODE = :EmployeeCode";

            using (var cmd1 = new OracleCommand(sql1, conn))
            {
                cmd1.Parameters.Add(new OracleParameter("EmployeeCode", EmployeeCode));

                using var reader1 = await cmd1.ExecuteReaderAsync();

                while (await reader1.ReadAsync())   // ReadAsync all rows, not just first
                {
                    // Capture UNIT_CODE once (same for all rows)

                    branchDetails.Add(new BranchDetail
                    {
                        BranchCode = reader1["BranchCode"]?.ToString(),
                        BranchName = reader1["BranchName"]?.ToString()
                    });
                    RoleDetails.Add(new RoleDetails
                    {
                        RoleId = "1",
                        RoleName = "Executive"

                    });
                }

                // If no rows found — employee is not an active executive
                if (branchDetails.Count == 0)
                {
                    return null;
                }
            }

            // ?? Second Query: Get user session details ??
            var sql2 = @"SELECT L.""userid"" AS USERID, L.HR_CODE, L.COM_CODE, L.STATUS,
                            E.EMP_CODE, E.NAME, E.DESIG, E.UNIT_CODE AS BRAN_CODE,
                            E.MOBILE, E.EMAIL, E.ZONE, E.REPORT_TO
                     FROM LOGIN L
                     LEFT JOIN HR_EMP_MST E ON E.EMP_CODE = L.HR_CODE
                     WHERE E.EMP_CODE = :EmployeeCode
                       AND L.STATUS = 'A'";

            using var cmd2 = new OracleCommand(sql2, conn);
            cmd2.Parameters.Add(new OracleParameter("EmployeeCode", EmployeeCode));

            using var reader2 = await cmd2.ExecuteReaderAsync();
            if (await reader2.ReadAsync())
            {
                var data = new UserSessionModel
                {
                    UserId = reader2["USERID"]?.ToString(),
                    HrCode = reader2["HR_CODE"]?.ToString(),
                    ComCode = reader2["COM_CODE"]?.ToString(),
                    EmpCode = reader2["EMP_CODE"]?.ToString(),
                    EmpName = reader2["NAME"]?.ToString(),
                    Designation = reader2["DESIG"]?.ToString(),
                    BranchCode = reader2["BRAN_CODE"]?.ToString(),
                    Mobile = reader2["MOBILE"]?.ToString(),
                    Email = reader2["EMAIL"]?.ToString(),
                    Zone = reader2["ZONE"]?.ToString(),
                    ReportTo = reader2["REPORT_TO"]?.ToString(),
                    // ?? Carry forward values from first query ??
                    UnitCode = reader2["BRAN_CODE"]?.ToString(),
                    BranchDetails = branchDetails,
                    RoleDetails = RoleDetails,
                    RoleName = RoleDetails.FirstOrDefault()?.RoleName ?? "Executive"
                };

                return data;
            }

            return null; // No matching login record found
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckUserExecutive: {ex.Message}");
            return null;
        }
    }

    public async Task<UserSessionModel> CheckUserHO(string EmployeeCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        try
        {
            // ?? First Query: Get all branch details from executive master ??
            var RoleDetails = new List<RoleDetails>();

            var branchDetails = new List<BranchDetail>();

            var sql1 = @"SELECT  CEM.BRANCH_CODE AS BranchCode, JPCM.""Pub_Cent_name"" AS BranchName
                     FROM APP_CIR_HO_APPROVAL_MAST CEM 
                     LEFT JOIN PUB_CENT_MAST JPCM ON JPCM.""Pub_cent_Code"" = CEM.BRANCH_CODE
                     WHERE IS_ACTIVE = 'Y'                       
                       AND EMPLOYEE_CODE = :EmployeeCode";

            using (var cmd1 = new OracleCommand(sql1, conn))
            {
                cmd1.Parameters.Add(new OracleParameter("EmployeeCode", EmployeeCode));

                using var reader1 = await cmd1.ExecuteReaderAsync();

                while (await reader1.ReadAsync())   // ReadAsync all rows, not just first
                {
                    // Capture UNIT_CODE once (same for all rows)

                    branchDetails.Add(new BranchDetail
                    {
                        BranchCode = reader1["BranchCode"]?.ToString(),
                        BranchName = reader1["BranchName"]?.ToString()
                    });
                    RoleDetails.Add(new RoleDetails
                    {
                        RoleId = "7",
                        RoleName = "HO"

                    });
                }

                // If no rows found — employee is not an active executive
                if (branchDetails.Count == 0)
                {
                    return null;
                }
            }

            // ?? Second Query: Get user session details ??
            var sql2 = @"SELECT L.""userid"" AS USERID, L.HR_CODE, L.COM_CODE, L.STATUS,
                            E.EMP_CODE, E.NAME, E.DESIG, E.UNIT_CODE AS BRAN_CODE,
                            E.MOBILE, E.EMAIL, E.ZONE, E.REPORT_TO
                     FROM LOGIN L
                     LEFT JOIN HR_EMP_MST E ON E.EMP_CODE = L.HR_CODE
                     WHERE E.EMP_CODE = :EmployeeCode
                       AND L.STATUS = 'A'";

            using var cmd2 = new OracleCommand(sql2, conn);
            cmd2.Parameters.Add(new OracleParameter("EmployeeCode", EmployeeCode));

            using var reader2 = await cmd2.ExecuteReaderAsync();
            if (await reader2.ReadAsync())
            {
                var data = new UserSessionModel
                {
                    UserId = reader2["USERID"]?.ToString(),
                    HrCode = reader2["HR_CODE"]?.ToString(),
                    ComCode = reader2["COM_CODE"]?.ToString(),
                    EmpCode = reader2["EMP_CODE"]?.ToString(),
                    EmpName = reader2["NAME"]?.ToString(),
                    Designation = reader2["DESIG"]?.ToString(),
                    BranchCode = reader2["BRAN_CODE"]?.ToString(),
                    Mobile = reader2["MOBILE"]?.ToString(),
                    Email = reader2["EMAIL"]?.ToString(),
                    Zone = reader2["ZONE"]?.ToString(),
                    ReportTo = reader2["REPORT_TO"]?.ToString(),
                    // ?? Carry forward values from first query ??
                    UnitCode = reader2["BRAN_CODE"]?.ToString(),
                    BranchDetails = branchDetails,
                    RoleDetails = RoleDetails,
                    RoleName = RoleDetails.FirstOrDefault()?.RoleName ?? "Executive"
                };

                return data;
            }

            return null; // No matching login record found
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckUserExecutive: {ex.Message}");
            return null;
        }
    }
    // QUERY 3: Forgot password
    public async Task<(string? email, string? password)> GetForgotPasswordAsync(string empId)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        using var cmd = new OracleCommand("SELECT EMAIL, SUPPLY_ALTERATION_PASSWORD FROM LOGIN WHERE HR_CODE = :EMPID", conn);
        cmd.Parameters.Add(new OracleParameter("EMPID", empId));
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (reader["EMAIL"]?.ToString(), reader["SUPPLY_ALTERATION_PASSWORD"]?.ToString());
        }
        return (null, null);
    }

    // QUERY 4: Profile
    public async Task<ProfileViewModel?> GetProfileAsync(string empCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT HEM1.EMP_CODE, HEM1.NAME, HEM1.REPORT_TO, PCM.""Pub_Cent_name"" AS BRANCH, HEM1.DEPARTMENT,
                    HEM1.EMP_CURRENT_STATUS, HEM1.ZONE,
                    HEM2.NAME AS REPORTINGPERSONNAME,
                    CPH.NAME AS ROLEPOSITION
                    FROM HR_EMP_MST HEM1
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code""=HEM1.UNIT_CODE
                    LEFT JOIN HR_EMP_MST HEM2 ON HEM1.REPORT_TO = HEM2.EMP_CODE
                    LEFT JOIN CIR_PLI_HIERARCHY_MAST CPHM ON CPHM.EMPLOYEE_CODE = HEM1.EMP_CODE
                    LEFT JOIN CIR_PLI_HIERARCHY CPH ON CPH.CODE = CPHM.HIERARCHY_CODE
                    WHERE HEM1.EMP_CODE = :EMPCODE";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMPCODE", empCode));
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ProfileViewModel
            {
                EmpCode = reader["EMP_CODE"]?.ToString(),
                Name = reader["NAME"]?.ToString(),
                ReportTo = reader["REPORT_TO"]?.ToString(),
                Branch = reader["BRANCH"]?.ToString(),
                Department = reader["DEPARTMENT"]?.ToString(),
                EmpCurrentStatus = reader["EMP_CURRENT_STATUS"] as decimal?,
                Zone = reader["ZONE"]?.ToString(),
                ReportingPersonName = reader["REPORTINGPERSONNAME"]?.ToString(),
                RolePosition = reader["ROLEPOSITION"]?.ToString()
            };
        }
        return null;
    }

    // QUERY 5: SE Dashboard stats
    public async Task<(int pending, int approved, int rejected, int today)> GetSEStatsAsync(string seUserId, string compCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT
                    COUNT(CASE WHEN STATUS = 'PENDING_ZH' THEN 1 END) AS PENDING,
                    COUNT(CASE WHEN STATUS = 'HO_APPROVED' THEN 1 END) AS APPROVED,
                    COUNT(CASE WHEN STATUS IN ('REJECTED','ZH_REJECTED') THEN 1 END) AS REJECTED,
                    COUNT(CASE WHEN TRUNC(CREATION_DATE) = TRUNC(SYSDATE) THEN 1 END) AS TODAY
                    FROM APP_CIR_SUPPLY_REQ
                    WHERE USERID = :SE_USERID AND COMP_CODE = :COMP_CODE";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("SE_USERID", seUserId));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToInt32(reader["PENDING"]),
                Convert.ToInt32(reader["APPROVED"]),
                Convert.ToInt32(reader["REJECTED"]),
                Convert.ToInt32(reader["TODAY"])
            );
        }
        return (0, 0, 0, 0);
    }

    // QUERY 6: SE Recent requests
    public async Task<List<SupplyRequestViewModel>> GetSERecentRequestsAsync(string seUserId, string compCode)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = "SELECT * FROM (" +
            " SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN," +
            " R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY, LL.HR_CODE AS EMP_CODE," +
            " R.STATUS, R.CREATION_DATE, R.REASON_CODE," +
            " R.CHANGED_SUPPLY_DATE, R.REMARKS," +
            " (SELECT AG_NAME FROM CIR_AGMAST WHERE AGCD = R.AGCD " +
            " AND DPCD = R.DPCD AND COMP_CODE = R.COMP_CODE AND UNIT= R.UNIT_CODE AND ROWNUM = 1) AS AG_NAME," +
            " (SELECT FF.DROP_POINT_NAME FROM CIR_AGMAST MM INNER " +
            " JOIN CIR_DROP_POINT_MAST FF ON MM.STATION_CODE = FF.DROP_POINT" +
            " WHERE MM.AGCD = R.AGCD AND MM.DPCD = R.DPCD AND MM.COMP_CODE = R.COMP_CODE AND ROWNUM = 1) AS DROP_POINT_NAME," +
            " LL.FIRSTNAME || ' ' || LL.LASTNAME AS CREATION_BY" +
            " FROM APP_CIR_SUPPLY_REQ R" +
            " LEFT JOIN LOGIN LL ON LL.HR_CODE = R.USERID" +
            " WHERE R.USERID = :SE_USERID AND R.COMP_CODE = :COMP_CODE" +
            " ORDER BY R.CREATION_DATE DESC" +
            ") WHERE ROWNUM <= 10";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("SE_USERID", seUserId));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequest(reader));
        }
        return list;
    }

    // QUERY 7: Agent lookup
    public async Task<AgentLookupViewModel?> GetAgentAsync(string agcd, string compCode, List<string?>? branchCodes)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }

        var sql = $@"SELECT AGCD, DPCD, AG_NAME,UNIT AS BRANCH_CODE, EXECUTIVE_CODE,
                        SUSPEND, SUPPLY_STOP_FLAG, MOBILE_NO1, ADDR1
                        FROM CIR_AGMAST
                        WHERE AGCD = :AGCD AND COMP_CODE = :COMP_CODE AND SUSPEND = 'N'
                        AND (SUPPLY_STOP_FLAG IS NULL OR SUPPLY_STOP_FLAG = 'N')
                        AND UNIT IN ({string.Join(",", branchParams)})";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("AGCD", agcd));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new AgentLookupViewModel
            {
                Agcd = reader["AGCD"]?.ToString(),
                Dpcd = reader["DPCD"]?.ToString(),
                AgName = reader["AG_NAME"]?.ToString(),
                BranchCode = reader["BRANCH_CODE"]?.ToString(),
                ExecutiveCode = reader["EXECUTIVE_CODE"]?.ToString(),
                Suspend = reader["SUSPEND"]?.ToString(),
                SupplyStopFlag = reader["SUPPLY_STOP_FLAG"]?.ToString(),
                MobileNo1 = reader["MOBILE_NO1"] as decimal?,
                Addr1 = reader["ADDR1"]?.ToString()
            };
        }
        return null;
    }

    // Agent search by name or code
    public async Task<List<object>> SearchAgentsAsync(string keyword, string compCode)
    {
        var list = new List<object>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT * FROM (
                    SELECT CA.AGCD, CA.DPCD, CA.AG_NAME, CA.UNIT AS BRANCH_CODE,PCM.""Pub_cent_Code"",PCM.""Pub_Cent_name"" 
                    FROM CIR_AGMAST CA 
                    LEFT JOIN PUB_CENT_MAST PCM 
                    ON PCM.""Pub_cent_Code"" =  CA.UNIT
                    WHERE CA.COMP_CODE = :COMP_CODE AND CA.SUSPEND = 'N'
                    AND (CA.SUPPLY_STOP_FLAG IS NULL OR CA.SUPPLY_STOP_FLAG = 'N')
                    AND (UPPER(CA.AG_NAME) LIKE UPPER(:KEYWORD) OR UPPER(CA.AGCD) LIKE UPPER(:KEYWORD))
                    ORDER BY AG_NAME
                    ) WHERE ROWNUM <= 15";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        cmd.Parameters.Add(new OracleParameter("KEYWORD", "%" + keyword + "%"));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new
            {
                agcd = reader["AGCD"]?.ToString(),
                dpcd = reader["DPCD"]?.ToString(),
                agName = reader["AG_NAME"]?.ToString(),
                branchCode = reader["Pub_cent_Code"]?.ToString(),
                branchname = reader["Pub_Cent_name"]?.ToString()
            });
        }
        return list;
    }

    // Agent search filtered by user's branch codes
    public async Task<List<object>> SearchAgentsByBranchAsync(string keyword, string compCode, List<string?>? branchCodes)
    {
        if (branchCodes == null || branchCodes.Count == 0)
            return await SearchAgentsAsync(keyword, compCode);

        var list = new List<object>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }
            

       var sql = $@"SELECT * FROM (
    SELECT CA.AGCD,
           CA.DPCD,
           CA.AG_NAME,
           CA.UNIT AS BRANCH_CODE,
           PCM.""Pub_cent_Code"",
           PCM.""Pub_Cent_name"",
           (
               SELECT FF.DROP_POINT_NAME
               FROM CIR_AGMAST MM
               INNER JOIN CIR_DROP_POINT_MAST FF
                   ON MM.STATION_CODE = FF.DROP_POINT
               WHERE MM.AGCD = CA.AGCD
                 AND MM.DPCD = CA.DPCD
                 AND MM.COMP_CODE = CA.COMP_CODE
                 AND ROWNUM = 1
           ) AS DROP_POINT_NAME
    FROM CIR_AGMAST CA
    LEFT JOIN PUB_CENT_MAST PCM
        ON PCM.""Pub_cent_Code"" = CA.UNIT
    WHERE CA.COMP_CODE = :COMP_CODE
      AND (CA.SUPPLY_STOP_FLAG IS NULL OR CA.SUPPLY_STOP_FLAG = 'N')
      AND (UPPER(CA.AG_NAME) LIKE UPPER(:KEYWORD)
           OR UPPER(CA.AGCD) LIKE UPPER(:KEYWORD))
      AND PCM.""Pub_cent_Code"" IN ({string.Join(",", branchParams)})
    ORDER BY CA.DPCD ASC, CA.AG_NAME ASC
) 
WHERE ROWNUM <= 15";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        cmd.Parameters.Add(new OracleParameter("KEYWORD", "%" + keyword + "%"));
        //for (int i = 0; i < branchCodes.Count; i++)
        //    cmd.Parameters.Add(new OracleParameter($"BRANCH{i}", branchCodes[i] ?? ""));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new
            {
                agcd = reader["AGCD"]?.ToString(),
                dpcd = reader["DPCD"]?.ToString(),
                agName = reader["AG_NAME"]?.ToString(),
                branchCode = reader["Pub_cent_Code"]?.ToString(),
                branchname = reader["Pub_Cent_name"]?.ToString(),
                droppointname = reader["DROP_POINT_NAME"]?.ToString()
            });
        }
        return list;
    }

    // QUERY 8: Current supply
    public async Task<List<SupplyViewModel>> GetSupplyAsync(string agcd, string dpcd, string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }
        var sql = $@"SELECT BASE_SUPPLY, SUPPLY_MON, SUPPLY_TUE, SUPPLY_WED,
                    SUPPLY_THU, SUPPLY_FRI, SUPPLY_SAT, SUPPLY_SUN,
                    SUPPLY_EFFECTIVE_DATE, SUPPLY_FLAG, PUBL, EDTN, SUPPLY_TYPE_CODE
                    FROM CIR_SUPPLY
                    WHERE AGCD = :AGCD AND DPCD = :DPCD AND COMP_CODE = :COMP_CODE AND SUPPLY_FLAG = 'Y'                    
                    AND UNIT IN ({string.Join(",", branchParams)})
                    ORDER BY PUBL, EDTN ";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("AGCD", agcd));
        cmd.Parameters.Add(new OracleParameter("DPCD", dpcd));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new SupplyViewModel
            {
                BaseSupply = reader["BASE_SUPPLY"] != DBNull.Value ? Convert.ToInt32(reader["BASE_SUPPLY"]) : 0,
                SupplyMon = reader["SUPPLY_MON"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_MON"]) : null,
                SupplyTue = reader["SUPPLY_TUE"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_TUE"]) : null,
                SupplyWed = reader["SUPPLY_WED"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_WED"]) : null,
                SupplyThu = reader["SUPPLY_THU"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_THU"]) : null,
                SupplyFri = reader["SUPPLY_FRI"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_FRI"]) : null,
                SupplySat = reader["SUPPLY_SAT"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_SAT"]) : null,
                SupplySun = reader["SUPPLY_SUN"] != DBNull.Value ? Convert.ToInt32(reader["SUPPLY_SUN"]) : null,
                SupplyEffectiveDate = reader["SUPPLY_EFFECTIVE_DATE"] as DateTime?,
                SupplyFlag = reader["SUPPLY_FLAG"]?.ToString(),
                Publ = reader["PUBL"]?.ToString(),
                Edtn = reader["EDTN"]?.ToString(),
                SupplyTypeCode = reader["SUPPLY_TYPE_CODE"]?.ToString()
            });
        }
        return list;
    }

    // Check if a pending request exists for the same agency
    public async Task<bool> HasPendingRequestAsync(string agcd, string dpcd, string compCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT COUNT(*) FROM APP_CIR_SUPPLY_REQ
                    WHERE AGCD = :AGCD AND DPCD = :DPCD AND COMP_CODE = :COMP_CODE
                    AND STATUS IN ('PENDING_ZH','PENDING_HO')";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("AGCD", agcd));
        cmd.Parameters.Add(new OracleParameter("DPCD", dpcd));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    // QUERY 9: Submit new supply request
    public async Task<bool> SubmitRequestAsync(SupplyRequestViewModel model)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                var sql = @"INSERT INTO APP_CIR_SUPPLY_REQ (
                            REQ_ID, COMP_CODE, UNIT_CODE, AGCD, DPCD,
                            PUBL, EDTN, SUPPLY_TYPE_CODE,
                            BASE_SUPPLY, INC_DEC, CHANGED_SUPPLY,
                            REASON_CODE, ZONE_CODE, USERID, CREATION_DATE,
                            CHANGED_SUPPLY_DATE, REMARKS, STATUS, ERP_PUSH_FLAG,
                            SUPPLY_MON, SUPPLY_TUE, SUPPLY_WED, SUPPLY_THU, SUPPLY_FRI, SUPPLY_SAT, SUPPLY_SUN, IS_DAYWISE_SUPPLY
                            ) VALUES (
                            SEQ_SUPPLY_REQ.NEXTVAL, :COMP_CODE, :UNIT_CODE, :AGCD, :DPCD,
                            :PUBL, :EDTN, :SUPPLY_TYPE_CODE,
                            :BASE_SUPPLY, :INC_DEC, :CHANGED_SUPPLY,
                            :REASON_CODE, :ZONE_CODE, :SE_USERID, SYSDATE,
                            :CHANGED_SUPPLY_DATE, :REMARKS, 'PENDING_ZH', 'N',
                            :SUPPLY_MON, :SUPPLY_TUE, :SUPPLY_WED, :SUPPLY_THU, :SUPPLY_FRI, :SUPPLY_SAT, :SUPPLY_SUN, :IS_DAYWISE_SUPPLY)";
                using var cmd = new OracleCommand(sql, conn) { Transaction = txn };
                cmd.Parameters.Add(new OracleParameter("COMP_CODE", model.CompCode));
                cmd.Parameters.Add(new OracleParameter("UNIT_CODE", model.UnitCode));
                cmd.Parameters.Add(new OracleParameter("AGCD", model.Agcd));
                cmd.Parameters.Add(new OracleParameter("DPCD", model.Dpcd));
                cmd.Parameters.Add(new OracleParameter("PUBL", model.Publ));
                cmd.Parameters.Add(new OracleParameter("EDTN", model.Edtn));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_TYPE_CODE", model.SupplyTypeCode));
                cmd.Parameters.Add(new OracleParameter("BASE_SUPPLY", model.BaseSupply));
                cmd.Parameters.Add(new OracleParameter("INC_DEC", model.IncDec));
                cmd.Parameters.Add(new OracleParameter("CHANGED_SUPPLY", model.ChangedSupply));
                cmd.Parameters.Add(new OracleParameter("REASON_CODE", model.ReasonCode));
                cmd.Parameters.Add(new OracleParameter("ZONE_CODE", model.ZoneCode));
                cmd.Parameters.Add(new OracleParameter("SE_USERID", model.EmployeeCode));
                cmd.Parameters.Add(new OracleParameter("CHANGED_SUPPLY_DATE", model.ChangedSupplyDate));
                cmd.Parameters.Add(new OracleParameter("REMARKS", model.Remarks));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_MON", model.SupplyMon ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_TUE", model.SupplyTue ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_WED", model.SupplyWed ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_THU", model.SupplyThu ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_FRI", model.SupplyFri ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_SAT", model.SupplySat ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("SUPPLY_SUN", model.SupplySun ?? (object)DBNull.Value));
                cmd.Parameters.Add(new OracleParameter("IS_DAYWISE_SUPPLY", model.IsDaywiseSupply));
                await cmd.ExecuteNonQueryAsync();
                txn.Commit();
                return true;
            }
            catch
            {
                txn.Rollback();
                return false;
            }
        }
        catch(Exception ex)
        { return false; }
    }

    // QUERY 10: SE full history
    public async Task<List<SupplyRequestViewModel>> GetSEHistoryAsync(string seUserId, string compCode)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT R.REQ_ID, R.AGCD, R.PUBL, R.EDTN,
                    R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.STATUS, R.CREATION_DATE, R.CHANGED_SUPPLY_DATE, R.REASON_CODE, R.REMARKS,
                    A.AG_NAME,
                    AP.ZH_ACTION AS APPR_ACTION, AP.ZH_ACTION_BY AS ACTION_BY, AP.ZH_ACTION_DATE AS ACTION_DATE, AP.ZH_REMARKS AS APPROVER_REMARKS,
                    AP.HO_ACTION, AP.HO_ACTION_BY, AP.HO_ACTION_DATE, AP.HO_REMARKS,
                    HEM.NAME AS CREATION_BY, HEM.EMP_CODE
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD  AND A.UNIT = R.UNIT_CODE
                    AND A.COMP_CODE = R.COMP_CODE
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    LEFT JOIN LOGIN LGN ON LGN.HR_CODE = R.USERID
                    LEFT JOIN HR_EMP_MST HEM ON LGN.HR_CODE = HEM.EMP_CODE
                    WHERE R.USERID = :SE_USERID AND R.COMP_CODE = :COMP_CODE
                    ORDER BY R.CREATION_DATE DESC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("SE_USERID", seUserId));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequestWithApproval(reader));
        }
        return list;
    }

    // QUERY 11: ZH stats
    public async Task<(int awaitingMe, int atHo, int approved, int rejected)> GetZHStatsAsync(string empCode, string compCode, List<string?>? branchCodes)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }

        var sql = $@"
        SELECT
            COUNT(CASE WHEN R.STATUS = 'PENDING_ZH' THEN 1 END) AS AWAITING_ME,
            COUNT(CASE WHEN R.STATUS = 'PENDING_HO' THEN 1 END) AS AT_HO,
            COUNT(CASE WHEN AP.ZH_ACTION = 'APPROVED' AND AP.ZH_ACTION_BY = :EMP_CODE THEN 1 END) AS APPROVED,
            COUNT(CASE WHEN R.STATUS IN ('REJECTED', 'ZH_REJECTED') THEN 1 END) AS REJECTED
        FROM APP_CIR_SUPPLY_REQ R
        LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
        WHERE R.COMP_CODE = :COMP_CODE
          AND R.UNIT_CODE IN ({string.Join(",", branchParams)})";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToInt32(reader["AWAITING_ME"]),
                Convert.ToInt32(reader["AT_HO"]),
                Convert.ToInt32(reader["APPROVED"]),
                Convert.ToInt32(reader["REJECTED"])
            );
        }
        return (0, 0, 0, 0);
    }

    // ZH Approved requests (from APP_CIR_SUPPLY_APPROVAL)
    public async Task<List<SupplyRequestViewModel>> GetZHApprovedByMeAsync(string empCode, string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");

        var sql = $@"
        SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
               R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
               R.REASON_CODE, R.REMARKS, R.USERID,
               R.CREATION_DATE, R.CHANGED_SUPPLY_DATE, R.STATUS,
               A.AG_NAME, A.UNIT AS BRANCH_CODE,
               PCM.""Pub_Cent_name"" AS BRANCH_NAME,
               HEM.NAME AS CREATION_BY, HEM.EMP_CODE,
               AP.ZH_ACTION_DATE AS ACTION_DATE, AP.ZH_REMARKS AS APPROVER_REMARKS,
               (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME
        FROM APP_CIR_SUPPLY_REQ R
        INNER JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
            AND AP.ZH_ACTION = 'APPROVED' AND AP.ZH_ACTION_BY = :EMP_CODE
        LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.COMP_CODE = R.COMP_CODE AND A.UNIT = R.UNIT_CODE
        LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
        LEFT JOIN HR_EMP_MST HEM ON HEM.EMP_CODE = R.USERID
        WHERE R.COMP_CODE = :COMP_CODE
          AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
        ORDER BY AP.ZH_ACTION_DATE DESC";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = MapSupplyRequest(reader);
            item.ActionDate = reader["ACTION_DATE"] as DateTime?;
            item.ApproverRemarks = reader["APPROVER_REMARKS"]?.ToString();
            list.Add(item);
        }
        return list;
    }

    // ZH At HO requests (approved by ZH, pending at HO)
    public async Task<List<SupplyRequestViewModel>> GetZHAtHoAsync(string empCode, string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");

        var sql = $@"
        SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
               R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
               R.REASON_CODE, R.REMARKS, R.USERID,
               R.CREATION_DATE, R.CHANGED_SUPPLY_DATE, R.STATUS,
               A.AG_NAME, A.UNIT AS BRANCH_CODE,
               PCM.""Pub_Cent_name"" AS BRANCH_NAME,
               HEM.NAME AS CREATION_BY, HEM.EMP_CODE,
               (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME
        FROM APP_CIR_SUPPLY_REQ R
        LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.COMP_CODE = R.COMP_CODE AND A.UNIT = R.UNIT_CODE
        LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
        LEFT JOIN HR_EMP_MST HEM ON HEM.EMP_CODE = R.USERID
        WHERE R.STATUS = 'PENDING_HO'
          AND R.COMP_CODE = :COMP_CODE
          AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
        ORDER BY R.CREATION_DATE DESC";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapSupplyRequest(reader));
        return list;
    }

    // ZH Rejected requests
    public async Task<List<SupplyRequestViewModel>> GetZHRejectedAsync(string empCode, string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");

        var sql = $@"
        SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
               R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
               R.REASON_CODE, R.REMARKS, R.USERID,
               R.CREATION_DATE, R.CHANGED_SUPPLY_DATE, R.STATUS,
               A.AG_NAME, A.UNIT AS BRANCH_CODE,
               PCM.""Pub_Cent_name"" AS BRANCH_NAME,
               HEM.NAME AS CREATION_BY, HEM.EMP_CODE,
               (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME
        FROM APP_CIR_SUPPLY_REQ R
        LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.COMP_CODE = R.COMP_CODE AND A.UNIT = R.UNIT_CODE
        LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
        LEFT JOIN HR_EMP_MST HEM ON HEM.EMP_CODE = R.USERID
        WHERE R.STATUS IN ('REJECTED','ZH_REJECTED')
          AND R.COMP_CODE = :COMP_CODE
          AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
        ORDER BY R.CREATION_DATE DESC";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(MapSupplyRequest(reader));
        return list;
    }

    // QUERY 12: ZH pending requests
    public async Task<List<SupplyRequestViewModel>> GetZHPendingAsync(string empCode, string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }

        var sql = $@"
        SELECT 
            R.REQ_ID,
            R.AGCD,
            R.DPCD,
            R.PUBL,
            R.EDTN,
            R.BASE_SUPPLY,
            R.INC_DEC,
            R.CHANGED_SUPPLY,
            R.REASON_CODE,
            R.REMARKS,
            R.USERID,
            R.CREATION_DATE,
            R.CHANGED_SUPPLY_DATE,
            R.STATUS,
            A.AG_NAME,
            A.UNIT AS BRANCH_CODE,
            HEM.NAME AS CREATION_BY,
            HEM.EMP_CODE,
            PCM.""Pub_Cent_name"" AS BRANCH_NAME,
            (
                SELECT FF.DROP_POINT_NAME
                FROM CIR_DROP_POINT_MAST FF
                WHERE FF.DROP_POINT = A.STATION_CODE
                AND ROWNUM = 1
            ) AS DROP_POINT_NAME
        FROM APP_CIR_SUPPLY_REQ R
        LEFT JOIN CIR_AGMAST A 
            ON A.AGCD = R.AGCD
           AND A.DPCD = R.DPCD
             AND A.UNIT = R.UNIT_CODE
           AND A.COMP_CODE = R.COMP_CODE
        LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
        LEFT JOIN HR_EMP_MST HEM ON HEM.EMP_CODE = R.USERID
        WHERE R.STATUS = 'PENDING_ZH'
          AND R.COMP_CODE = :COMP_CODE
          AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
        ORDER BY R.CREATION_DATE ASC";

        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequest(reader));
        }
        return list;
    }    // QUERY 13: ZH Approve/Reject
    // Logic: If increase <= 10% of base supply, ZH approval pushes directly to ERP.
    //        If increase > 10% or decrease, forward to HO for second-level approval.
    public async Task<bool> ZHApproveRejectAsync(decimal reqId, string action, string zhUserId, string remarks, string compCode)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                if (action == "APPROVED")
                {
                    // Fetch request details to determine routing
                    var sqlFetch = @"SELECT INC_DEC, BASE_SUPPLY, CHANGED_SUPPLY, UNIT_CODE, AGCD, DPCD, PUBL, EDTN, SUPPLY_TYPE_CODE, CHANGED_SUPPLY_DATE,
                                     IS_DAYWISE_SUPPLY, SUPPLY_MON, SUPPLY_TUE, SUPPLY_WED, SUPPLY_THU, SUPPLY_FRI, SUPPLY_SAT, SUPPLY_SUN
                                     FROM APP_CIR_SUPPLY_REQ WHERE REQ_ID = :REQ_ID";
                    using var cmdFetch = new OracleCommand(sqlFetch, conn) { Transaction = txn };
                    cmdFetch.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                    using var rdr = await cmdFetch.ExecuteReaderAsync();
                    if (!await rdr.ReadAsync()) { txn.Rollback(); return false; }

                    var incDec = rdr["INC_DEC"]?.ToString();
                    var baseSupply = rdr["BASE_SUPPLY"] != DBNull.Value ? Convert.ToDecimal(rdr["BASE_SUPPLY"]) : 0m;
                    var changedSupply = rdr["CHANGED_SUPPLY"] != DBNull.Value ? Convert.ToDecimal(rdr["CHANGED_SUPPLY"]) : 0m;
                    var unitCode = rdr["UNIT_CODE"]?.ToString() ?? "";
                    var agcd = rdr["AGCD"]?.ToString() ?? "";
                    var dpcd = rdr["DPCD"]?.ToString() ?? "";
                    var publ = rdr["PUBL"]?.ToString() ?? "";
                    var edtn = rdr["EDTN"]?.ToString() ?? "";
                    var supplyTypeCode = rdr["SUPPLY_TYPE_CODE"]?.ToString() ?? "";
                    var changedSupplyDate = rdr["CHANGED_SUPPLY_DATE"] as DateTime?;
                    var isDaywiseSupply = rdr["IS_DAYWISE_SUPPLY"] != DBNull.Value ? Convert.ToInt32(rdr["IS_DAYWISE_SUPPLY"]) : 0;
                    int? zhSupMon = rdr["SUPPLY_MON"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_MON"]) : null;
                    int? zhSupTue = rdr["SUPPLY_TUE"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_TUE"]) : null;
                    int? zhSupWed = rdr["SUPPLY_WED"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_WED"]) : null;
                    int? zhSupThu = rdr["SUPPLY_THU"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_THU"]) : null;
                    int? zhSupFri = rdr["SUPPLY_FRI"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_FRI"]) : null;
                    int? zhSupSat = rdr["SUPPLY_SAT"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_SAT"]) : null;
                    int? zhSupSun = rdr["SUPPLY_SUN"] != DBNull.Value ? Convert.ToInt32(rdr["SUPPLY_SUN"]) : null;
                    rdr.Close();

                    // Determine if ZH-only approval (increase <= 10%)
                    bool zhOnlyApproval = false;
                    if (incDec == "I" && baseSupply > 0)
                    {
                        var increaseAmount = changedSupply - baseSupply;
                        var tenPercent = baseSupply * 0.10m;
                        zhOnlyApproval = increaseAmount <= tenPercent;
                    }

                    string zhToStatus = zhOnlyApproval ? "HO_APPROVED" : "PENDING_HO";
                    string reqStatus = zhOnlyApproval ? "HO_APPROVED" : "PENDING_HO";

                    // Insert single approval row with ZH data
                    var sqlApproval = @"INSERT INTO APP_CIR_SUPPLY_APPROVAL (
                                    APPROVAL_ID, REQ_ID, COMP_CODE, ZH_ACTION, ZH_ACTION_BY,
                                    ZH_ACTION_DATE, ZH_REMARKS, ZH_FROM_STATUS, ZH_TO_STATUS, STATUS
                                    ) VALUES (
                                    SEQ_SUPPLY_APPROVAL.NEXTVAL, :REQ_ID, :COMP_CODE, :ZH_ACTION, :ZH_USERID,
                                    SYSDATE, :ZH_REMARKS, 'PENDING_ZH', :ZH_TO_STATUS, :STATUS)";
                    using var cmdApproval = new OracleCommand(sqlApproval, conn) { Transaction = txn };
                    cmdApproval.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                    cmdApproval.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
                    cmdApproval.Parameters.Add(new OracleParameter("ZH_ACTION", action));
                    cmdApproval.Parameters.Add(new OracleParameter("ZH_USERID", zhUserId));
                    cmdApproval.Parameters.Add(new OracleParameter("ZH_REMARKS", remarks));
                    cmdApproval.Parameters.Add(new OracleParameter("ZH_TO_STATUS", zhToStatus));
                    cmdApproval.Parameters.Add(new OracleParameter("STATUS", reqStatus));
                    await cmdApproval.ExecuteNonQueryAsync();

                    // Update request status
                    var sql2 = $"UPDATE APP_CIR_SUPPLY_REQ SET STATUS = :STATUS, ERP_PUSH_FLAG = 'N' WHERE REQ_ID = :REQ_ID";
                    using var cmd2 = new OracleCommand(sql2, conn) { Transaction = txn };
                    cmd2.Parameters.Add(new OracleParameter("STATUS", reqStatus));
                    cmd2.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                    await cmd2.ExecuteNonQueryAsync();

                    if (zhOnlyApproval)
                    {
                        // Push to ERP (update CIR_SUPPLY)
                        string sql3;
                        if (isDaywiseSupply == 1)
                        {
                            sql3 = @"UPDATE CIR_SUPPLY SET
                                    BASE_SUPPLY=:CHANGED_SUPPLY, SUPPLY_MON=:SUPPLY_MON, SUPPLY_TUE=:SUPPLY_TUE,
                                    SUPPLY_WED=:SUPPLY_WED, SUPPLY_THU=:SUPPLY_THU, SUPPLY_FRI=:SUPPLY_FRI,
                                    SUPPLY_SAT=:SUPPLY_SAT, SUPPLY_SUN=:SUPPLY_SUN,
                                    SUPPLY_EFFECTIVE_DATE=:CHANGED_SUPPLY_DATE, UPDATED_BY=:ZH_USERID, UPDATED_DT=SYSDATE
                                    WHERE COMP_CODE=:COMP_CODE AND UNIT=:UNIT_CODE AND AGCD=:AGCD AND DPCD=:DPCD
                                    AND PUBL=:PUBL AND EDTN=:EDTN AND SUPPLY_TYPE_CODE=:SUPPLY_TYPE_CODE";
                        }
                        else
                        {
                            sql3 = @"UPDATE CIR_SUPPLY SET
                                    BASE_SUPPLY=:CHANGED_SUPPLY,
                                    SUPPLY_EFFECTIVE_DATE=:CHANGED_SUPPLY_DATE, UPDATED_BY=:ZH_USERID, UPDATED_DT=SYSDATE
                                    WHERE COMP_CODE=:COMP_CODE AND UNIT=:UNIT_CODE AND AGCD=:AGCD AND DPCD=:DPCD
                                    AND PUBL=:PUBL AND EDTN=:EDTN AND SUPPLY_TYPE_CODE=:SUPPLY_TYPE_CODE";
                        }
                        using var cmd3 = new OracleCommand(sql3, conn) { Transaction = txn };
                        cmd3.Parameters.Add(new OracleParameter("CHANGED_SUPPLY", changedSupply));
                        if (isDaywiseSupply == 1)
                        {
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_MON", zhSupMon ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_TUE", zhSupTue ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_WED", zhSupWed ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_THU", zhSupThu ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_FRI", zhSupFri ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_SAT", zhSupSat ?? (object)DBNull.Value));
                            cmd3.Parameters.Add(new OracleParameter("SUPPLY_SUN", zhSupSun ?? (object)DBNull.Value));
                        }
                        cmd3.Parameters.Add(new OracleParameter("CHANGED_SUPPLY_DATE", changedSupplyDate));
                        cmd3.Parameters.Add(new OracleParameter("ZH_USERID", zhUserId));
                        cmd3.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
                        cmd3.Parameters.Add(new OracleParameter("UNIT_CODE", unitCode));
                        cmd3.Parameters.Add(new OracleParameter("AGCD", agcd));
                        cmd3.Parameters.Add(new OracleParameter("DPCD", dpcd));
                        cmd3.Parameters.Add(new OracleParameter("PUBL", publ));
                        cmd3.Parameters.Add(new OracleParameter("EDTN", edtn));
                        cmd3.Parameters.Add(new OracleParameter("SUPPLY_TYPE_CODE", supplyTypeCode));
                        await cmd3.ExecuteNonQueryAsync();

                        // Mark ERP pushed
                        var sql4a = "UPDATE APP_CIR_SUPPLY_REQ SET ERP_PUSH_FLAG='Y', ERP_PUSH_DATE=SYSDATE WHERE REQ_ID=:REQ_ID";
                        using var cmd4a = new OracleCommand(sql4a, conn) { Transaction = txn };
                        cmd4a.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                        await cmd4a.ExecuteNonQueryAsync();

                        // Update approval row with ERP push info
                        var sql4b = "UPDATE APP_CIR_SUPPLY_APPROVAL SET ERP_PUSHED_BY=:ZH_USERID, ERP_PUSHED_DATE=SYSDATE WHERE REQ_ID=:REQ_ID";
                        using var cmd4b = new OracleCommand(sql4b, conn) { Transaction = txn };
                        cmd4b.Parameters.Add(new OracleParameter("ZH_USERID", zhUserId));
                        cmd4b.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                        await cmd4b.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    // Rejection - Insert single row with ZH rejection
                    var sqlApproval = @"INSERT INTO APP_CIR_SUPPLY_APPROVAL (
                                APPROVAL_ID, REQ_ID, COMP_CODE, ZH_ACTION, ZH_ACTION_BY,
                                ZH_ACTION_DATE, ZH_REMARKS, ZH_FROM_STATUS, ZH_TO_STATUS, STATUS
                                ) VALUES (
                                SEQ_SUPPLY_APPROVAL.NEXTVAL, :REQ_ID, :COMP_CODE, :ZH_ACTION, :ZH_USERID,
                                SYSDATE, :ZH_REMARKS, 'PENDING_ZH', 'ZH_REJECTED', 'ZH_REJECTED')";
                    using var cmd1 = new OracleCommand(sqlApproval, conn) { Transaction = txn };
                    cmd1.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                    cmd1.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
                    cmd1.Parameters.Add(new OracleParameter("ZH_ACTION", action));
                    cmd1.Parameters.Add(new OracleParameter("ZH_USERID", zhUserId));
                    cmd1.Parameters.Add(new OracleParameter("ZH_REMARKS", remarks));
                    await cmd1.ExecuteNonQueryAsync();

                    var sql2 = "UPDATE APP_CIR_SUPPLY_REQ SET STATUS = 'ZH_REJECTED' WHERE REQ_ID = :REQ_ID";
                    using var cmd2 = new OracleCommand(sql2, conn) { Transaction = txn };
                    cmd2.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                    await cmd2.ExecuteNonQueryAsync();
                }

                txn.Commit();
                return true;
            }
            catch { txn.Rollback(); return false; }
        }
        catch { return false; }
    }

    // QUERY 14: HO stats
    public async Task<(int awaitingHo, int hoApproved, int totalIncreased, int totalDecreased, int hoRejected)> GetHOStatsAsync(string compCode, DateTime selectedDate, List<string?>? branchCodes, string empCode = "")
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }

        var sql = $@"SELECT
                    COUNT(CASE WHEN R.STATUS = 'PENDING_HO' THEN 1 END) AS AWAITING_HO,
                    COUNT(CASE WHEN R.STATUS = 'HO_APPROVED' AND AP.HO_ACTION_BY = :EMP_CODE THEN 1 END) AS HO_APPROVED,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'I' AND R.STATUS = 'HO_APPROVED' AND AP.HO_ACTION_BY = :EMP_CODE THEN (R.CHANGED_SUPPLY - R.BASE_SUPPLY) ELSE 0 END),0) AS TOTAL_INCREASED,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'D' AND R.STATUS = 'HO_APPROVED' AND AP.HO_ACTION_BY = :EMP_CODE THEN (R.BASE_SUPPLY - R.CHANGED_SUPPLY) ELSE 0 END),0) AS TOTAL_DECREASED,
                    COUNT(CASE WHEN R.STATUS = 'HO_REJECTED' AND AP.HO_ACTION_BY = :EMP_CODE THEN 1 END) AS HO_REJECTED
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    WHERE R.COMP_CODE = :COMP_CODE
                    AND R.UNIT_CODE IN ({string.Join(",", branchParams)})";

        using var cmd = new OracleCommand(sql, conn);
        cmd.BindByName = true;
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                Convert.ToInt32(reader["AWAITING_HO"]),
                Convert.ToInt32(reader["HO_APPROVED"]),
                Convert.ToInt32(reader["TOTAL_INCREASED"]),
                Convert.ToInt32(reader["TOTAL_DECREASED"]),
                Convert.ToInt32(reader["HO_REJECTED"])
            );
        }
        return (0, 0, 0, 0, 0);
    }

    // QUERY 15: HO pending list
    public async Task<List<SupplyRequestViewModel>> GetHOPendingAsync(string compCode, List<string?>? branchCodes)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
        {
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");
        }


        var sql = $@"SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
                    R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.REASON_CODE, R.ZONE_CODE, R.USERID, R.CREATION_DATE, R.CHANGED_SUPPLY_DATE,
                    A.AG_NAME, A.UNIT AS BRANCH_CODE,
                    AP.ZH_ACTION_BY AS ZH_APPROVED_BY,
                    AP.ZH_REMARKS AS ZH_REMARKS,
                    AP.ZH_ACTION_DATE AS ZH_ACTION_DATE, R.STATUS, HEM.EMP_CODE, HEM.NAME AS CREATION_BY,
                    AP.ZH_REMARKS AS APPROVER_REMARKS, 
                    (SELECT FF.DROP_POINT_NAME FROM CIR_AGMAST MM INNER 
                    JOIN CIR_DROP_POINT_MAST FF ON MM.STATION_CODE = FF.DROP_POINT
                    WHERE MM.AGCD = R.AGCD AND MM.DPCD = R.DPCD AND MM.COMP_CODE = R.COMP_CODE 
                    AND MM.UNIT=R.UNIT_CODE AND ROWNUM = 1) AS DROP_POINT_NAME,
                    PCM.""Pub_Cent_name"" AS BRANCH_NAME,
                     R.UNIT_CODE, R.SUPPLY_TYPE_CODE
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    LEFT JOIN hr_emp_mst HEM ON HEM.EMP_CODE = R.USERID
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.COMP_CODE = R.COMP_CODE 
                    AND A.UNIT = R.UNIT_CODE
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
                    WHERE R.STATUS = 'PENDING_HO' AND R.COMP_CODE = :COMP_CODE
                    AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
                    ORDER BY R.CREATION_DATE ASC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var item = MapSupplyRequest(reader);
            item.ZhApprovedBy = reader["ZH_APPROVED_BY"]?.ToString();
            item.ZhRemarks = reader["ZH_REMARKS"]?.ToString();
            item.ZhActionDate = reader["ZH_ACTION_DATE"] as DateTime?;
            list.Add(item);
        }
        return list;
    }

    // QUERY 16: HO Approve (4-step transaction)
    public async Task<bool> HOApproveAsync(decimal reqId, string hoUserId, string remarks, string compCode, string unitCode, string agcd, string dpcd, string publ, string edtn, string supplyTypeCode, decimal changedSupply, DateTime? changedSupplyDate)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                // Step 1: Update existing approval row with HO data
                var sql1 = @"UPDATE APP_CIR_SUPPLY_APPROVAL SET
                            HO_ACTION = 'APPROVED', HO_ACTION_BY = :HO_USERID,
                            HO_ACTION_DATE = SYSDATE, HO_REMARKS = :REMARKS,
                            HO_FROM_STATUS = 'PENDING_HO', HO_TO_STATUS = 'HO_APPROVED',
                            STATUS = 'HO_APPROVED'
                            WHERE REQ_ID = :REQ_ID";
                using var cmd1 = new OracleCommand(sql1, conn) { Transaction = txn };
                cmd1.Parameters.Add(new OracleParameter("HO_USERID", hoUserId));
                cmd1.Parameters.Add(new OracleParameter("REMARKS", remarks));
                cmd1.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd1.ExecuteNonQueryAsync();

                // Step 2
                var sql2 = "UPDATE APP_CIR_SUPPLY_REQ SET STATUS='HO_APPROVED', ERP_PUSH_FLAG='N' WHERE REQ_ID=:REQ_ID";
                using var cmd2 = new OracleCommand(sql2, conn) { Transaction = txn };
                cmd2.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd2.ExecuteNonQueryAsync();

                // Step 3: Check if day-wise supply is enabled
                int isDaywiseSupply = 0;
                int? supMon = null, supTue = null, supWed = null, supThu = null, supFri = null, supSat = null, supSun = null;
                var sqlDaywise = @"SELECT IS_DAYWISE_SUPPLY, SUPPLY_MON, SUPPLY_TUE, SUPPLY_WED, SUPPLY_THU, SUPPLY_FRI, SUPPLY_SAT, SUPPLY_SUN
                                   FROM APP_CIR_SUPPLY_REQ WHERE REQ_ID = :REQ_ID";
                using var cmdDaywise = new OracleCommand(sqlDaywise, conn) { Transaction = txn };
                cmdDaywise.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                using var rdrDaywise = await cmdDaywise.ExecuteReaderAsync();
                if (await rdrDaywise.ReadAsync())
                {
                    isDaywiseSupply = rdrDaywise["IS_DAYWISE_SUPPLY"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["IS_DAYWISE_SUPPLY"]) : 0;
                    if (isDaywiseSupply == 1)
                    {
                        supMon = rdrDaywise["SUPPLY_MON"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_MON"]) : null;
                        supTue = rdrDaywise["SUPPLY_TUE"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_TUE"]) : null;
                        supWed = rdrDaywise["SUPPLY_WED"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_WED"]) : null;
                        supThu = rdrDaywise["SUPPLY_THU"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_THU"]) : null;
                        supFri = rdrDaywise["SUPPLY_FRI"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_FRI"]) : null;
                        supSat = rdrDaywise["SUPPLY_SAT"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_SAT"]) : null;
                        supSun = rdrDaywise["SUPPLY_SUN"] != DBNull.Value ? Convert.ToInt32(rdrDaywise["SUPPLY_SUN"]) : null;
                    }
                }
                rdrDaywise.Close();

                string sql3;
                if (isDaywiseSupply == 1)
                {
                    sql3 = @"UPDATE CIR_SUPPLY SET
                            BASE_SUPPLY=:CHANGED_SUPPLY, SUPPLY_MON=:SUPPLY_MON, SUPPLY_TUE=:SUPPLY_TUE,
                            SUPPLY_WED=:SUPPLY_WED, SUPPLY_THU=:SUPPLY_THU, SUPPLY_FRI=:SUPPLY_FRI,
                            SUPPLY_SAT=:SUPPLY_SAT, SUPPLY_SUN=:SUPPLY_SUN,
                            SUPPLY_EFFECTIVE_DATE=SYSDATE, UPDATED_BY=:HO_USERID, UPDATED_DT=SYSDATE
                            WHERE COMP_CODE=:COMP_CODE AND UNIT=:UNIT_CODE AND AGCD=:AGCD AND DPCD=:DPCD
                            AND PUBL=:PUBL AND EDTN=:EDTN AND SUPPLY_TYPE_CODE=:SUPPLY_TYPE_CODE";
                }
                else
                {
                    sql3 = @"UPDATE CIR_SUPPLY SET
                            BASE_SUPPLY=:CHANGED_SUPPLY,
                            SUPPLY_EFFECTIVE_DATE=SYSDATE, UPDATED_BY=:HO_USERID, UPDATED_DT=SYSDATE
                            WHERE COMP_CODE=:COMP_CODE AND UNIT=:UNIT_CODE AND AGCD=:AGCD AND DPCD=:DPCD
                            AND PUBL=:PUBL AND EDTN=:EDTN AND SUPPLY_TYPE_CODE=:SUPPLY_TYPE_CODE";
                }
                using var cmd3 = new OracleCommand(sql3, conn) { Transaction = txn };
                cmd3.Parameters.Add(new OracleParameter("CHANGED_SUPPLY", changedSupply));
                if (isDaywiseSupply == 1)
                {
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_MON", supMon ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_TUE", supTue ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_WED", supWed ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_THU", supThu ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_FRI", supFri ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_SAT", supSat ?? (object)DBNull.Value));
                    cmd3.Parameters.Add(new OracleParameter("SUPPLY_SUN", supSun ?? (object)DBNull.Value));
                }
                cmd3.Parameters.Add(new OracleParameter("HO_USERID", hoUserId));
                cmd3.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
                cmd3.Parameters.Add(new OracleParameter("UNIT_CODE", unitCode));
                cmd3.Parameters.Add(new OracleParameter("AGCD", agcd));
                cmd3.Parameters.Add(new OracleParameter("DPCD", dpcd));
                cmd3.Parameters.Add(new OracleParameter("PUBL", publ));
                cmd3.Parameters.Add(new OracleParameter("EDTN", edtn));
                cmd3.Parameters.Add(new OracleParameter("SUPPLY_TYPE_CODE", supplyTypeCode));
                await cmd3.ExecuteNonQueryAsync();

                // Step 4a
                var sql4a = "UPDATE APP_CIR_SUPPLY_REQ SET ERP_PUSH_FLAG='Y', ERP_PUSH_DATE=SYSDATE WHERE REQ_ID=:REQ_ID";
                using var cmd4a = new OracleCommand(sql4a, conn) { Transaction = txn };
                cmd4a.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd4a.ExecuteNonQueryAsync();

                // Step 4b: Update approval row with ERP push info
                var sql4b = "UPDATE APP_CIR_SUPPLY_APPROVAL SET ERP_PUSHED_BY=:HO_USERID, ERP_PUSHED_DATE=SYSDATE WHERE REQ_ID=:REQ_ID";
                using var cmd4b = new OracleCommand(sql4b, conn) { Transaction = txn };
                cmd4b.Parameters.Add(new OracleParameter("HO_USERID", hoUserId));
                cmd4b.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd4b.ExecuteNonQueryAsync();

                txn.Commit();
                return true;
            }
            catch { txn.Rollback(); return false; }
        }
        catch(Exception ex)
        { 
            return false;
        }
    }

    // HO Reject
    public async Task<bool> HORejectAsync(decimal reqId, string hoUserId, string remarks, string compCode)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                // Update existing approval row with HO rejection
                var sql1 = @"UPDATE APP_CIR_SUPPLY_APPROVAL SET
                            HO_ACTION = 'REJECTED', HO_ACTION_BY = :HO_USERID,
                            HO_ACTION_DATE = SYSDATE, HO_REMARKS = :REMARKS,
                            HO_FROM_STATUS = 'PENDING_HO', HO_TO_STATUS = 'HO_REJECTED',
                            STATUS = 'HO_REJECTED'
                            WHERE REQ_ID = :REQ_ID";
                using var cmd1 = new OracleCommand(sql1, conn) { Transaction = txn };
                cmd1.Parameters.Add(new OracleParameter("HO_USERID", hoUserId));
                cmd1.Parameters.Add(new OracleParameter("REMARKS", remarks));
                cmd1.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd1.ExecuteNonQueryAsync();

                var sql2 = "UPDATE APP_CIR_SUPPLY_REQ SET STATUS='HO_REJECTED' WHERE REQ_ID=:REQ_ID";
                using var cmd2 = new OracleCommand(sql2, conn) { Transaction = txn };
                cmd2.Parameters.Add(new OracleParameter("REQ_ID", reqId));
                await cmd2.ExecuteNonQueryAsync();

                txn.Commit();
                return true;
            }
            catch { txn.Rollback(); return false; }
        }
        catch { return false; }
    }

    // QUERY 17: Branch-wise summary
    public async Task<List<BranchSummaryViewModel>> GetBranchSummaryAsync(string compCode, DateTime selectedDate)
    {
        var list = new List<BranchSummaryViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT R.ZONE_CODE, A.UNIT AS BRANCH_CODE,
                    COUNT(*) AS TOTAL_REQUESTS,
                    COUNT(CASE WHEN R.INC_DEC = 'I' THEN 1 END) AS INCREASES,
                    COUNT(CASE WHEN R.INC_DEC = 'D' THEN 1 END) AS DECREASES,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'I' THEN R.CHANGED_SUPPLY - R.BASE_SUPPLY ELSE 0 END),0) AS NET_INC_COPIES,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'D' THEN R.BASE_SUPPLY - R.CHANGED_SUPPLY ELSE 0 END),0) AS NET_DEC_COPIES,
                    COUNT(CASE WHEN R.ERP_PUSH_FLAG = 'Y' THEN 1 END) AS PUSHED_TO_ERP
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD
                    WHERE R.COMP_CODE = :COMP_CODE AND TRUNC(R.CREATION_DATE) = TRUNC(:SELECTED_DATE)
                    GROUP BY R.ZONE_CODE, A.UNIT
                    ORDER BY R.ZONE_CODE";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        cmd.Parameters.Add(new OracleParameter("SELECTED_DATE", selectedDate));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BranchSummaryViewModel
            {
                ZoneCode = reader["ZONE_CODE"]?.ToString(),
                BranchCode = reader["BRANCH_CODE"]?.ToString(),
                TotalRequests = Convert.ToInt32(reader["TOTAL_REQUESTS"]),
                Increases = Convert.ToInt32(reader["INCREASES"]),
                Decreases = Convert.ToInt32(reader["DECREASES"]),
                NetIncCopies = Convert.ToInt32(reader["NET_INC_COPIES"]),
                NetDecCopies = Convert.ToInt32(reader["NET_DEC_COPIES"]),
                PushedToErp = Convert.ToInt32(reader["PUSHED_TO_ERP"])
            });
        }
        return list;
    }

    // Branch-wise summary filtered by allowed branches (for HO)
    public async Task<List<BranchSummaryViewModel>> GetBranchSummaryByBranchesAsync(string compCode, DateTime selectedDate, List<string?>? branchCodes)
    {
        var list = new List<BranchSummaryViewModel>();
        if (branchCodes == null || branchCodes.Count == 0)
            return list;

        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchParams = new List<string>();
        for (int i = 0; i < branchCodes.Count; i++)
            branchParams.Add("'" + (branchCodes[i] ?? "").Replace("'", "''") + "'");

        var sql = $@"SELECT R.UNIT_CODE AS BRANCH_CODE, PCM.""Pub_Cent_name"" AS BRANCH_NAME,
                    COUNT(*) AS TOTAL_REQUESTS,
                    COUNT(CASE WHEN R.INC_DEC = 'I' THEN 1 END) AS INCREASES,
                    COUNT(CASE WHEN R.INC_DEC = 'D' THEN 1 END) AS DECREASES,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'I' THEN R.CHANGED_SUPPLY - R.BASE_SUPPLY ELSE 0 END),0) AS NET_INC_COPIES,
                    NVL(SUM(CASE WHEN R.INC_DEC = 'D' THEN R.BASE_SUPPLY - R.CHANGED_SUPPLY ELSE 0 END),0) AS NET_DEC_COPIES,
                    COUNT(CASE WHEN R.ERP_PUSH_FLAG = 'Y' THEN 1 END) AS PUSHED_TO_ERP
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
                    WHERE R.COMP_CODE = :COMP_CODE 
                    AND TRUNC(R.CREATION_DATE) = TRUNC(:SELECTED_DATE)
                    AND R.UNIT_CODE IN ({string.Join(",", branchParams)})
                    GROUP BY R.UNIT_CODE, PCM.""Pub_Cent_name""
                    ORDER BY R.UNIT_CODE";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        cmd.Parameters.Add(new OracleParameter("SELECTED_DATE", selectedDate));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new BranchSummaryViewModel
            {
                BranchCode = reader["BRANCH_CODE"]?.ToString(),
                BranchName = reader["BRANCH_NAME"]?.ToString(),
                TotalRequests = Convert.ToInt32(reader["TOTAL_REQUESTS"]),
                Increases = Convert.ToInt32(reader["INCREASES"]),
                Decreases = Convert.ToInt32(reader["DECREASES"]),
                NetIncCopies = Convert.ToInt32(reader["NET_INC_COPIES"]),
                NetDecCopies = Convert.ToInt32(reader["NET_DEC_COPIES"]),
                PushedToErp = Convert.ToInt32(reader["PUSHED_TO_ERP"])
            });
        }
        return list;
    }

    // QUERY 18: ERP push log
    public async Task<List<ErpPushLogViewModel>> GetErpPushLogAsync(string compCode, DateTime selectedDate)
    {
        var list = new List<ErpPushLogViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT R.REQ_ID, R.AGCD, A.AG_NAME,
                    R.PUBL, R.EDTN, R.BASE_SUPPLY, R.CHANGED_SUPPLY, R.INC_DEC,
                    R.ERP_PUSH_DATE, AP.ERP_PUSHED_BY AS PUSHED_BY
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    WHERE R.COMP_CODE = :COMP_CODE AND R.ERP_PUSH_FLAG = 'Y'
                    AND TRUNC(R.ERP_PUSH_DATE) = TRUNC(:SELECTED_DATE)
                    ORDER BY R.ERP_PUSH_DATE DESC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        cmd.Parameters.Add(new OracleParameter("SELECTED_DATE", selectedDate));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new ErpPushLogViewModel
            {
                ReqId = Convert.ToDecimal(reader["REQ_ID"]),
                Agcd = reader["AGCD"]?.ToString(),
                AgName = reader["AG_NAME"]?.ToString(),
                Publ = reader["PUBL"]?.ToString(),
                Edtn = reader["EDTN"]?.ToString(),
                BaseSupply = reader["BASE_SUPPLY"] as decimal?,
                ChangedSupply = reader["CHANGED_SUPPLY"] as decimal?,
                IncDec = reader["INC_DEC"]?.ToString(),
                ErpPushDate = reader["ERP_PUSH_DATE"] as DateTime?,
                PushedBy = reader["PUSHED_BY"]?.ToString()
            });
        }
        return list;
    }

    // QUERY 19: Audit trail
    public async Task<List<AuditTrailViewModel>> GetAuditTrailAsync(decimal reqId)
    {
        var list = new List<AuditTrailViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT R.REQ_ID, R.AGCD, A.AG_NAME,
                    R.PUBL, R.EDTN, R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.REASON_CODE, R.USERID AS SUBMITTED_BY, R.CREATION_DATE, R.STATUS,
                    R.REMARKS, R.ZONE_CODE, R.CHANGED_SUPPLY_DATE,
                    AP.ZH_ACTION, AP.ZH_ACTION_BY, AP.ZH_ACTION_DATE, AP.ZH_REMARKS,
                    AP.ZH_FROM_STATUS, AP.ZH_TO_STATUS,
                    AP.HO_ACTION, AP.HO_ACTION_BY, AP.HO_ACTION_DATE, AP.HO_REMARKS,
                    AP.HO_FROM_STATUS, AP.HO_TO_STATUS,
                    AP.ERP_PUSHED_BY, AP.ERP_PUSHED_DATE,
                    A.UNIT AS BRANCH_CODE, PCM.""Pub_Cent_name"",
                    HEM.NAME AS SUBMITTED_BY_NAME, HEM.EMP_CODE
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = A.UNIT 
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    LEFT JOIN Login LGN ON R.USERID = LGN.HR_CODE 
                    LEFT JOIN hr_emp_mst HEM ON LGN.HR_CODE = HEM.EMP_CODE
                    WHERE R.REQ_ID = :REQ_ID";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("REQ_ID", reqId));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new AuditTrailViewModel
            {
                ReqId = Convert.ToDecimal(reader["REQ_ID"]),
                Agcd = reader["AGCD"]?.ToString(),
                AgName = reader["AG_NAME"]?.ToString(),
                Publ = reader["PUBL"]?.ToString(),
                Edtn = reader["EDTN"]?.ToString(),
                BaseSupply = Convert.ToInt32(reader["BASE_SUPPLY"]),
                IncDec = reader["INC_DEC"]?.ToString(),
                ChangedSupply = Convert.ToInt32(reader["CHANGED_SUPPLY"]),
                ReasonCode = reader["REASON_CODE"]?.ToString(),
                Remarks = reader["REMARKS"]?.ToString(),
                ZoneCode = reader["ZONE_CODE"]?.ToString(),
                BranchCode = reader["BRANCH_CODE"]?.ToString(),
                ChangedSupplyDate = reader["CHANGED_SUPPLY_DATE"] as DateTime?,
                SubmittedBy = reader["SUBMITTED_BY"]?.ToString(),
                CreationDate = reader["CREATION_DATE"] as DateTime?,
                Status = reader["STATUS"]?.ToString(),
                ZhAction = reader["ZH_ACTION"]?.ToString(),
                ZhActionBy = reader["ZH_ACTION_BY"]?.ToString(),
                ZhActionDate = reader["ZH_ACTION_DATE"] as DateTime?,
                ZhRemarks = reader["ZH_REMARKS"]?.ToString(),
                ZhFromStatus = reader["ZH_FROM_STATUS"]?.ToString(),
                ZhToStatus = reader["ZH_TO_STATUS"]?.ToString(),
                HoAction = reader["HO_ACTION"]?.ToString(),
                HoActionBy = reader["HO_ACTION_BY"]?.ToString(),
                HoActionDate = reader["HO_ACTION_DATE"] as DateTime?,
                HoRemarks = reader["HO_REMARKS"]?.ToString(),
                HoFromStatus = reader["HO_FROM_STATUS"]?.ToString(),
                HoToStatus = reader["HO_TO_STATUS"]?.ToString(),
                ErpPushedBy = reader["ERP_PUSHED_BY"]?.ToString(),
                ErpPushedDate = reader["ERP_PUSHED_DATE"] as DateTime?,
                BranchName = reader["Pub_Cent_name"]?.ToString(),
                SUBMITTEDBYNAME = reader["SUBMITTED_BY_NAME"]?.ToString(),
                CreationByCode = reader["EMP_CODE"]?.ToString()
            });
        }
        return list;
    }

    // HO History (all requests)
    public async Task<List<SupplyRequestViewModel>> GetHOHistoryAsync(string compCode, List<string?>? branchCodes = null)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchFilter = "";
        if (branchCodes != null && branchCodes.Count > 0)
        {
            var branchParams = branchCodes.Select(b => "'" + (b ?? "").Replace("'", "''") + "'");
            branchFilter = $" AND R.UNIT_CODE IN ({string.Join(",", branchParams)})";
        }

        var sql = $@"SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
                    R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.STATUS, R.CREATION_DATE, R.REASON_CODE, R.REMARKS,
                    A.AG_NAME, R.UNIT_CODE,
                    PCM.""Pub_Cent_name"" AS BRANCH_NAME,
                    (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME,
                    AP.ZH_ACTION AS APPR_ACTION, AP.ZH_ACTION_BY AS ACTION_BY, AP.ZH_ACTION_DATE AS ACTION_DATE, AP.ZH_REMARKS AS APPROVER_REMARKS,
                    HEM.NAME AS CREATION_BY
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.UNIT = R.UNIT_CODE AND A.COMP_CODE = R.COMP_CODE
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID
                    
                    LEFT JOIN HR_EMP_MST HEM ON  HEM.EMP_CODE = R.USERID
                    WHERE R.COMP_CODE = :COMP_CODE{branchFilter}
                    ORDER BY R.CREATION_DATE DESC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequestWithApproval(reader));
        }
        return list;
    }
    public async Task<List<SupplyRequestViewModel>> GetHOHistoryApproveByMeAsync(string compCode, List<string?>? branchCodes = null)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchFilter = "";
        if (branchCodes != null && branchCodes.Count > 0)
        {
            var branchParams = branchCodes.Select(b => "'" + (b ?? "").Replace("'", "''") + "'");
            branchFilter = $" AND R.UNIT_CODE IN ({string.Join(",", branchParams)})";
        }

        var sql = $@"SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
                    R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.STATUS, R.CREATION_DATE, R.REASON_CODE, R.REMARKS,
                    A.AG_NAME, R.UNIT_CODE,
                    PCM.""Pub_Cent_name"" AS BRANCH_NAME,
                    (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME,
                    AP.HO_ACTION AS APPR_ACTION, AP.ERP_PUSHED_BY AS ACTION_BY, AP.ERP_PUSHED_DATE AS ACTION_DATE, AP.HO_REMARKS AS APPROVER_REMARKS,
                    HEM.NAME AS CREATION_BY
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.UNIT = R.UNIT_CODE AND A.COMP_CODE = R.COMP_CODE
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID AND AP.ERP_PUSHED_BY IS NOT NULL
                   
                    LEFT JOIN HR_EMP_MST HEM ON  HEM.EMP_CODE =R.USERID
                    WHERE R.COMP_CODE = :COMP_CODE{branchFilter}
                    ORDER BY R.CREATION_DATE DESC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequestWithApproval(reader));
        }
        return list;
    }
    public async Task<List<SupplyRequestViewModel>> GetHOHistoryIncreaseAndDecrease(string compCode, List<string?>? branchCodes = null)
    {
        var list = new List<SupplyRequestViewModel>();
        using var conn = GetConnection();
        await conn.OpenAsync();

        var branchFilter = "";
        if (branchCodes != null && branchCodes.Count > 0)
        {
            var branchParams = branchCodes.Select(b => "'" + (b ?? "").Replace("'", "''") + "'");
            branchFilter = $" AND R.UNIT_CODE IN ({string.Join(",", branchParams)})";
        }

        var sql = $@"SELECT R.REQ_ID, R.AGCD, R.DPCD, R.PUBL, R.EDTN,
                    R.BASE_SUPPLY, R.INC_DEC, R.CHANGED_SUPPLY,
                    R.STATUS, R.CREATION_DATE, R.REASON_CODE, R.REMARKS,
                    A.AG_NAME, R.UNIT_CODE,
                    PCM.""Pub_Cent_name"" AS BRANCH_NAME,
                    (SELECT FF.DROP_POINT_NAME FROM CIR_DROP_POINT_MAST FF WHERE FF.DROP_POINT = A.STATION_CODE AND ROWNUM = 1) AS DROP_POINT_NAME,
                    AP.HO_ACTION AS APPR_ACTION, AP.ERP_PUSHED_BY AS ACTION_BY, AP.ERP_PUSHED_DATE AS ACTION_DATE, AP.HO_REMARKS AS APPROVER_REMARKS,
                    HEM.NAME AS CREATION_BY
                    FROM APP_CIR_SUPPLY_REQ R
                    LEFT JOIN CIR_AGMAST A ON A.AGCD = R.AGCD AND A.DPCD = R.DPCD AND A.UNIT = R.UNIT_CODE AND A.COMP_CODE = R.COMP_CODE
                    LEFT JOIN PUB_CENT_MAST PCM ON PCM.""Pub_cent_Code"" = R.UNIT_CODE
                    LEFT JOIN APP_CIR_SUPPLY_APPROVAL AP ON AP.REQ_ID = R.REQ_ID AND AP.ERP_PUSHED_BY IS NOT NULL
                    LEFT JOIN HR_EMP_MST HEM ON  HEM.EMP_CODE = R.USERID
                    WHERE R.COMP_CODE = :COMP_CODE{branchFilter}
                    ORDER BY R.CREATION_DATE DESC";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("COMP_CODE", compCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSupplyRequestWithApproval(reader));
        }
        return list;
    }

    private SupplyRequestViewModel MapSupplyRequest(System.Data.Common.DbDataReader reader)
    {
        return new SupplyRequestViewModel
        {
            ReqId = reader["REQ_ID"] != DBNull.Value ? Convert.ToDecimal(reader["REQ_ID"]) : null,
            Agcd = reader["AGCD"]?.ToString(),
            Dpcd = HasColumn(reader, "DPCD") ? reader["DPCD"]?.ToString() : null,
            Publ = reader["PUBL"]?.ToString(),
            Edtn = reader["EDTN"]?.ToString(),
            BaseSupply = reader["BASE_SUPPLY"] != DBNull.Value ? Convert.ToDecimal(reader["BASE_SUPPLY"]) : null,
            IncDec = reader["INC_DEC"]?.ToString(),
            ChangedSupply = reader["CHANGED_SUPPLY"] != DBNull.Value ? Convert.ToDecimal(reader["CHANGED_SUPPLY"]) : null,
            Status = reader["STATUS"]?.ToString(),
            CreationDate = reader["CREATION_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["CREATION_DATE"]) : null,
            ReasonCode = HasColumn(reader, "REASON_CODE") ? reader["REASON_CODE"]?.ToString() : null,
            Remarks = HasColumn(reader, "REMARKS") ? reader["REMARKS"]?.ToString() : null,
            AgName = HasColumn(reader, "AG_NAME") ? reader["AG_NAME"]?.ToString() : null,
            DropPointName = HasColumn(reader, "DROP_POINT_NAME") ? reader["DROP_POINT_NAME"]?.ToString() : null,
            BranchName = HasColumn(reader, "BRANCH_NAME") ? reader["BRANCH_NAME"]?.ToString() : null,
            CreationBy = HasColumn(reader, "CREATION_BY") ? reader["CREATION_BY"]?.ToString() : null,
            CreationByCode = HasColumn(reader, "EMP_CODE") ? reader["EMP_CODE"]?.ToString() : null,
            BranchCode = HasColumn(reader, "UNIT_CODE") ? reader["UNIT_CODE"]?.ToString() : null,
            SupplyTypeCode = HasColumn(reader, "SUPPLY_TYPE_CODE") ? reader["SUPPLY_TYPE_CODE"]?.ToString() : null,
            



        };
    }

    private SupplyRequestViewModel MapSupplyRequestWithApproval(System.Data.Common.DbDataReader reader)
    {
        var item = MapSupplyRequest(reader);
        item.ApprAction = HasColumn(reader, "APPR_ACTION") ? reader["APPR_ACTION"]?.ToString() : null;
        item.ActionBy = HasColumn(reader, "ACTION_BY") ? reader["ACTION_BY"]?.ToString() : null;
        item.ActionDate = HasColumn(reader, "ACTION_DATE") ? reader["ACTION_DATE"] as DateTime? : null;
        item.ApproverRemarks = HasColumn(reader, "APPROVER_REMARKS") ? reader["APPROVER_REMARKS"]?.ToString() : null;
        return item;
    }

    private bool HasColumn(System.Data.Common.DbDataReader reader, string columnName)
    {
        try
        {
            reader.GetOrdinal(columnName);
            return true;
        }
        catch { return false; }
    }

    // Get HO user's allowed branch codes from APP_CIR_HO_APPROVAL_MAST
    public async Task<List<string>> GetHOAllowedBranchesAsync(string employeeCode)
    {
        var branches = new List<string>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT BRANCH_CODE FROM APP_CIR_HO_APPROVAL_MAST 
                    WHERE EMPLOYEE_CODE = :EMP_CODE AND IS_ACTIVE = 'Y'";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", employeeCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var branch = reader["BRANCH_CODE"]?.ToString();
            if (!string.IsNullOrEmpty(branch))
                branches.Add(branch);
        }
        return branches;
    }

    // Get all active HO user emails from APP_CIR_HO_APPROVAL_MAST
    public async Task<List<string>> GetHOEmailsAsync()
    {
        var emails = new List<string>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT DISTINCT EMAIL_ID FROM APP_CIR_HO_APPROVAL_MAST 
                    WHERE IS_ACTIVE = 'Y' AND EMAIL_ID IS NOT NULL";
        using var cmd = new OracleCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var email = reader["EMAIL_ID"]?.ToString();
            if (!string.IsNullOrEmpty(email))
                emails.Add(email);
        }
        return emails;
    }

    // Get ZH user emails for branches that a request belongs to
    public async Task<List<string>> GetZHEmailsByBranchAsync(string branchCode)
    {
        var emails = new List<string>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT DISTINCT HEM.EMAIL 
                    FROM HR_EMP_MST HEM
                    INNER JOIN CIR_PLI_HIERARCHY_MAST CPHM ON CPHM.EMPLOYEE_CODE = HEM.EMP_CODE
                    WHERE CPHM.HIERARCHY_CODE = '4'
                    AND HEM.UNIT_CODE = :BRANCH_CODE
                    AND HEM.EMAIL IS NOT NULL";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("BRANCH_CODE", branchCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var email = reader["EMAIL"]?.ToString();
            if (!string.IsNullOrEmpty(email))
                emails.Add(email);
        }
        return emails;
    }

    // Get email of a specific employee
    public async Task<string?> GetEmployeeEmailAsync(string empCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT EMAIL FROM HR_EMP_MST WHERE EMP_CODE = :EMP_CODE";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    // Get the branch (UNIT_CODE) of a specific request
    public async Task<string?> GetRequestBranchAsync(decimal reqId)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT UNIT_CODE FROM APP_CIR_SUPPLY_REQ WHERE REQ_ID = :REQ_ID";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("REQ_ID", reqId));
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    // Get the creator (USERID) of a specific request
    public async Task<string?> GetRequestCreatorAsync(decimal reqId)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT USERID FROM APP_CIR_SUPPLY_REQ WHERE REQ_ID = :REQ_ID";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("REQ_ID", reqId));
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    // Save push token to LOGIN table for a user
    public async Task SavePushTokenAsync(string empCode, string pushToken)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                var sql = @"UPDATE LOGIN SET PUSH_TOKEN = :PUSH_TOKEN WHERE HR_CODE = :EMP_CODE AND STATUS = 'A'";
                using var cmd = new OracleCommand(sql, conn) { Transaction = txn };
                cmd.Parameters.Add(new OracleParameter("PUSH_TOKEN", pushToken));
                cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
                await cmd.ExecuteNonQueryAsync();
                txn.Commit();
            }
            catch { txn.Rollback(); }
        }
        catch { }
    }

    // Clear push token on logout
    public async Task ClearPushTokenAsync(string empCode)
    {
        try
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var txn = conn.BeginTransaction();
            try
            {
                var sql = @"UPDATE LOGIN SET PUSH_TOKEN = NULL WHERE HR_CODE = :EMP_CODE";
                using var cmd = new OracleCommand(sql, conn) { Transaction = txn };
                cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
                await cmd.ExecuteNonQueryAsync();
                txn.Commit();
            }
            catch { txn.Rollback(); }
        }
        catch { }
    }

    // Get push token for a specific employee
    public async Task<string?> GetPushTokenByEmpCodeAsync(string empCode)
    {
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT PUSH_TOKEN FROM LOGIN WHERE HR_CODE = :EMP_CODE AND STATUS = 'A' AND PUSH_TOKEN IS NOT NULL";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("EMP_CODE", empCode));
        var result = await cmd.ExecuteScalarAsync();
        return result?.ToString();
    }

    // Get push tokens for all users with a specific role (hierarchy_code) in a specific branch
    public async Task<List<string>> GetPushTokensByRoleAndBranchAsync(string hierarchyCode, string branchCode)
    {
        var tokens = new List<string>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT DISTINCT L.PUSH_TOKEN 
                    FROM LOGIN L
                    INNER JOIN CIR_PLI_HIERARCHY_MAST CPHM ON CPHM.EMPLOYEE_CODE = L.HR_CODE
                    WHERE CPHM.HIERARCHY_CODE = :HIERARCHY_CODE
                    AND CPHM.UNIT_CODE = :BRANCH_CODE
                    AND CPHM.ISACTIVEFORPLI = 'Y'
                    AND L.STATUS = 'A'
                    AND L.PUSH_TOKEN IS NOT NULL";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("HIERARCHY_CODE", hierarchyCode));
        cmd.Parameters.Add(new OracleParameter("BRANCH_CODE", branchCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var token = reader["PUSH_TOKEN"]?.ToString();
            if (!string.IsNullOrEmpty(token))
                tokens.Add(token);
        }
        return tokens;
    }

    // Get push tokens for all users with a specific role (any branch)
    public async Task<List<string>> GetPushTokensByRoleAsync(string hierarchyCode)
    {
        var tokens = new List<string>();
        using var conn = GetConnection();
        await conn.OpenAsync();
        var sql = @"SELECT DISTINCT L.PUSH_TOKEN 
                    FROM LOGIN L
                    INNER JOIN CIR_PLI_HIERARCHY_MAST CPHM ON CPHM.EMPLOYEE_CODE = L.HR_CODE
                    WHERE CPHM.HIERARCHY_CODE = :HIERARCHY_CODE
                    AND CPHM.ISACTIVEFORPLI = 'Y'
                    AND L.STATUS = 'A'
                    AND L.PUSH_TOKEN IS NOT NULL";
        using var cmd = new OracleCommand(sql, conn);
        cmd.Parameters.Add(new OracleParameter("HIERARCHY_CODE", hierarchyCode));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var token = reader["PUSH_TOKEN"]?.ToString();
            if (!string.IsNullOrEmpty(token))
                tokens.Add(token);
        }
        return tokens;
    }
}
