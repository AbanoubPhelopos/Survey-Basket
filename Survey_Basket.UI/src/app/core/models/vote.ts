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
