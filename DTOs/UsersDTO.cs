namespace AssetRegistry.DTOs
{
    public class UsersDTO
    {
        public UsersView User { get; set; }
        public IEnumerable<UsersView> Users { get; set; }


    }

    public class UsersView
    {
        private string _designation;


        public string Id { get; set; }
        public string EncryptedId { get; set; }
        public string UserName { get; set; }
        //[Required(ErrorMessage = "FirstName is Required.")]
        public string FirstName { get; set; }
        //[Required(ErrorMessage = "LastName is Required.")]
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public string EmployeeCode { get; set; }
        public string HrisNo { get; set; }
        public string BarCode { get; set; }
        public string Nic { get; set; }
        public string UserSkill { get; set; }
        public string Supervisor { get; set; }
        public string SupervisorId { get; set; }
        public string SiteSupervisorId { get; set; }
        public string SiteSupervisor { get; set; }
        public int SiteId { get; set; }
        public string SiteLocation { get; set; }
        public string ProjectManager { get; set; }
        public string ProjectManagerId { get; set; }
        public string UserFullName { get; set; }
        public string Designation
        {
            //get;
            //set;
            get => _designation;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _designation = "Not Available";
                }
                else
                {
                    _designation = value;
                }
            }
        }
        public bool IsActive { get; set; }
        public bool LoginEnabled { get; set; }
        public bool FrAvailable { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int EmployeeCategoryId { get; set; }
        public string EmployeeCategory { get; set; }
        public bool EnableERPSync { get; set; }
        public string EpfNo { get; set; }
        public bool BankInformation { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string BankAccountNumber { get; set; }
        public bool PerDayEligibility { get; set; }
        public string ProjectName { get; set; }
        public string CustomerName { get; set; }
        public IEnumerable<UserClaim> UserClaims { get; set; }
        public List<string> SelectedClaims { get; set; }
        public object TwoFactorEnabled { get; set; }
        public object PhoneNumberConfirmed { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsReportEnabled { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string? LoggedInDevice { get; set; }

        public string GetUserStatus()
        {
            if (IsActive)
            {
                return "ACTIVE";
            }

            return "INACTIVE";
        }

    }

    public class ApplicationUserDTO
    {
        private string _designation;
        public string Id { get; set; }
        public string EncryptedId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public string Designation
        {
            //get;
            //set;
            get => _designation;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _designation = "Not Available";
                }
                else
                {
                    _designation = value;
                }
            }
        }
        public bool IsActive { get; set; }
    }

    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class MobileChangePassword
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class EmployeementInfo
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HrisNo { get; set; }
        public string NicNo { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public string Designation { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public EmployeeSupervisor Superior { get; set; }
        public IEnumerable<EmployeeSite> Sites { get; set; }

    }

    public class EmployeementInfoMob
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HrisNo { get; set; }
        public string NicNo { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Image { get; set; }
        public string Designation { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        //public EmployeeSupervisor Superior { get; set; }
        public List<EmployeeSuperior> Superior { get; set; }
        public IEnumerable<EmployeeSite> Sites { get; set; }

    }

    public class EmployeeSupervisor
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class EmployeeSuperior
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleId { get; set; }
    }

    public class EmployeeSite
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AttendanceSupervisor { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ActiveUsers
    {
        public string Id { get; set; }
        public string Username { get; set; }
    }

    public class SiteSuperiorForMob
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RoleId { get; set; }
    }
}
