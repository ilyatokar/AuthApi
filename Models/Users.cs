using System;
using System.ComponentModel.DataAnnotations;

public class Users
{
    public long Id {get; set;}
    public string Login { get; set; }
    public string Password { get; set; }
    public string Role {get; set;}
}