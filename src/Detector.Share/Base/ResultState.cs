namespace de.webducer.net.Detector.Base {
  public enum ResultState : byte {
    /// <summary>
    /// Not set
    /// </summary>
    None = 0,

    /// <summary>
    /// Task finished successfull
    /// </summary>
    Success = 1,

    /// <summary>
    /// User canceled the task
    /// </summary>
    UserCanceled = 2,

    /// <summary>
    /// Task finished with exception
    /// </summary>
    Exception = 3,

    /// <summary>
    /// Task finished with timeout
    /// </summary>
    TimeOut = 4,

    /// <summary>
    /// Detektor not finished
    /// </summary>
    DetectorNotFinished = 5,

    /// <summary>
    /// Detector task canceled
    /// </summary>
    TaskCanceled = 6,

    /// <summary>
    /// User definied state 1
    /// </summary>
    UserDefinied1 = 240,

    /// <summary>
    /// User definied state 2
    /// </summary>
    UserDefinied2 = 241,

    /// <summary>
    /// User definied state 3
    /// </summary>
    UserDefinied3 = 242,

    /// <summary>
    /// User definied state 4
    /// </summary>
    UserDefinied4 = 243,

    /// <summary>
    /// User definied state 5
    /// </summary>
    UserDefinied5 = 244,

    /// <summary>
    /// User definied state 6
    /// </summary>
    UserDefinied6 = 245,

    /// <summary>
    /// User definied state 7
    /// </summary>
    UserDefinied7 = 246,

    /// <summary>
    /// User definied state 8
    /// </summary>
    UserDefinied8 = 247,

    /// <summary>
    /// User definied state 9
    /// </summary>
    UserDefinied9 = 248,

    /// <summary>
    /// User definied state 10
    /// </summary>
    UserDefinied10 = 249,

    /// <summary>
    /// User definied state 11
    /// </summary>
    UserDefinied11 = 250,

    /// <summary>
    /// User definied state 12
    /// </summary>
    UserDefinied12 = 251,

    /// <summary>
    /// User definied state 13
    /// </summary>
    UserDefinied13 = 252,

    /// <summary>
    /// User definied state 14
    /// </summary>
    UserDefinied14 = 253,

    /// <summary>
    /// User definied state 15
    /// </summary>
    UserDefinied15 = 254,

    /// <summary>
    /// User definied state 16
    /// </summary>
    UserDefinied16 = 255
  }
}
