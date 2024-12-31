
///-------------------------------------------------------------------
///
///
///   Description:    Update Progress Model for Unity MDS Updater
///   Author:         Alex Kern, Vincent Uhlmann                  
///  
///
///-------------------------------------------------------------------

public class MDSxUpdateProgress
{
    public string Message { get; set; }
    public MDSxErrorType ErrorType { get; set; }

    public float EndVal { get; set; }
    public float Progress { get; set; }
}