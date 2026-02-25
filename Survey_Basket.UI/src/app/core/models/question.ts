export type QuestionType =
  | 'SingleChoice'
  | 'MultipleChoice'
  | 'TrueFalse'
  | 'Number'
  | 'Text'
  | 'FileUpload'
  | 'Country';

export interface QuestionOption {
  id: string;
  content: string;
}

export interface Question {
  id: string;
  content: string;
  type: QuestionType | number;
  isRequired: boolean;
  displayOrder: number;
  answers: QuestionOption[];
  isActive: boolean;
}

export interface QuestionRequest {
  content: string;
  type: QuestionType | number;
  isRequired: boolean;
  displayOrder: number;
  answers: string[];
}
