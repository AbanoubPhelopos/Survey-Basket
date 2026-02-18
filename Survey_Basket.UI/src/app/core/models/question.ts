export interface Question {
  id: string;
  content: string;
  answers: string[];
  isActive: boolean;
}

export interface QuestionRequest {
  content: string;
  answers: string[];
}
