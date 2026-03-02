export interface VoteRequest {
  answers: VoteAnswerRequest[];
}

export interface VoteAnswerRequest {
  questionId: string;
  answerId: string;
}

export interface QuestionResponse {
  id: string;
  content: string;
  answers: AnswerResponse[];
}

export interface AnswerResponse {
  id: string;
  content: string;
}

export interface MyVoteAnswerResponse {
  question: string;
  answer: string;
}

export interface MyVoteResponse {
  pollId: string;
  pollTitle: string;
  submittedOn: string;
  answers: MyVoteAnswerResponse[];
}
