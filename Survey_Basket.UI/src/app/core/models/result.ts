export interface PollVotesResponse {
  title: string;
  votes: VotesResponse[];
}

export interface VotesResponse {
  voterName: string;
  voteDate: string;
  selectedAnswers: QuestionAnswerResponse[];
}

export interface QuestionAnswerResponse {
  question: string;
  answer: string;
}

export interface VotesPerDayResponse {
  date: string;
  voteCount: number;
}

export interface VotesPerQuestionResponse {
  question: string;
  selectedAnswers: VotesPerAnswerResponse[];
}

export interface VotesPerAnswerResponse {
  answer: string;
  count: number;
}
