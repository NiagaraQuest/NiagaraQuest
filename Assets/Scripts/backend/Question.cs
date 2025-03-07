using UnityEngine;
using System;
using System.ComponentModel;

public abstract class Question {
    public int Id {get; set;}
    public string Category {get; set;}
    public string Qst {get; set;}
    public string Difficulty {get; set;}

}

public class QCMQuestion : Question
{
     public string[] Choices{get; set;}
     public int CorrectChoice {get; set;}

}

public class OpenQuestion : Question 
{
     public string Answer {set; get;}
     
}