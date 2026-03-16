using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EmployeeDatabase", menuName = "OS/Employee Database")]
public class EmployeeDatabase : ScriptableObject
{
    [System.Serializable]
    public class EmployeeData
    {
        public string employeeId;
        public string password;
        public string employeeName; // 选填：可以用于登录后的欢迎词
    }

    public List<EmployeeData> employees = new List<EmployeeData>();

    public bool ValidateLogin(string id, string pw)
    {
        // 查找是否存在匹配的工号和密码
        return employees.Exists(e => e.employeeId == id && e.password == pw);
    }
}