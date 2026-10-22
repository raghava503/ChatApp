using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatApp.Data;

namespace ChatApp.Models;


public class Message
{
    public int Id {get;set;}
    public string Room {get;set;} = "";
    public string User {get;set;} = "";
    public string Text {get;set;} = "";
    public DateTime SentAt {get;set;} = DateTime.UtcNow;
}
