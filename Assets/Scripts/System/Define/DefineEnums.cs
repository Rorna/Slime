

public enum FieldObjectTypeEnum
{
    Player,
    Enemy,
    BossEnemy,
    Item,
    Effect,
    Count,
}

public enum CameraFocusTypeEnum
{
    Normal,
    LookAround,
    FocusTarget,
    LookFocusedTarget,
    None,
}

public enum SceneTypeEnum
{
    Unknown,
    MainMenu,
    InGame,
    Count,
}

public enum FieldObjectStateEnum
{
    Idle,
    Move,
    Attack,
    MovingAttack,
    Dodge,
    Stun,
    Dead,
    Invincible,
    None,
    Count,
}

public enum StatValueEnum
{
    MaxHP,
    CurrentHP,
    MoveSpeed,
    ChangeMoveSpeed,
    DodgeSpeed,
    DodgeCooldown,
    ChangeDodgeSpeed,
    ScaleValue,
    ChangeScaleValue,
    DuringStunTime,
    AttackDelay,
    ThrowSpeed,
    SightRange,
    SightAngle,
    AttackRange,
    MaxSummonCount,
    Count
}

public enum LightAreaStateEnum
{
    Light,
    Normal,
    None,
    Count
}

public enum SoundTypeEnum
{
    Effect,
    BGM,
    Count
}

public enum GameTypeEnum
{
    Game,
    System,
    Count
}

public enum HotKeyCodeEnum
{
    Attack = 323, //MOUSE LEFT BUTTON
    Equip = 324, //MOUSE RIGHT BUTTON
    ThrowAttack = 325, //MOUSE CENTER BUTTON
    Dodge = 32, //SPACE BAR
    Interact = 102, // F
    Target = 101, //E
    LookAround = 304, //LEFT SHIFT
    Cancel = 27, //ESC
    Count,
}