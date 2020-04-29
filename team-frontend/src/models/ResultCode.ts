export enum ResultCode {
    /* Error codes */
    InvalidCode = 'InvalidCode',
    TeamNameIsTaken = 'TeamNameIsTaken',
    InvalidTeamId = 'InvalidTeamId',
    InvalidQuestionId = 'InvalidQuestionId',
    QuestionNotInQuiz = 'QuestionNotInQuiz',
    QuestionNotInCurrentQuizSection = 'QuestionNotInCurrentQuizSection',
    InvalidInteractionId = 'InvalidInteractionId',
    InvalidGameStateTransition = 'InvalidGameStateTransition',
    InvalidGameId = 'InvalidGameId',
    InvalidUserNameOrPassword = 'InvalidUserNameOrPassword',
    InvalidUserId = 'InvalidUserId',
    QuizMasterUnauthorizedForGame = 'QuizMasterUnauthorizedForGame',
    LobbyUnavailableBecauseOfGameState = 'LobbyUnavailableBecauseOfGameState',
    NoRoleClaimForUser = 'NoRoleClaimForUser',
    NoCurrentGameIdClaimForUser = 'NoCurrentGameIdClaimForUser',
    UnauthorizedRole = 'UnauthorizedRole',
    TeamAlreadyLoggedIn = 'TeamAlreadyLoggedIn',
    ValidationError = 'ValidationError',
    /* Success codes */
    ThatsYou = 'ThatsYou',
    TeamRegisteredAndLoggedIn = 'TeamRegisteredAndLoggedIn',
    LoggedOut = 'LoggedOut',
    TeamRenamed = 'TeamRenamed',
    TeamMembersChanged = 'TeamMembersChanged',
    TeamDeleted = 'TeamDeleted',
    UserLoggedIn = 'UserLoggedIn',
    InteractionResponseSubmitted = 'InteractionResponseSubmitted',
    AuthSuccessfullyTested = 'AuthSuccessfullyTested',
    GameStateChanged = 'GameStateChanged',
    GameSelected = 'GameSelected'
}
