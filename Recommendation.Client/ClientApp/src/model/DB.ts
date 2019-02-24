export enum UserType {
  Client = 1,
  Admin,
  Finance,
}

export interface SignedInUser {
  id: number;
  email: string;
  userType: UserType;
}

export interface User extends SignedInUser {
  membershipId: number;
}

export interface Movie {
  id: number;
  description: string;
  title: string;
  tags: Tag[];
  averageRating: number;
  imageUrl?: string;
}

export interface Tag {
  id: number;
  text: string;
}

export interface Membership {
  id: number;
  usesLeft: number;
  receipts?: Receipt[];
}

export enum ReceiptType {
  OneTimeRecommendation,
  Membership,
  ExtraRecommendation,
}

export interface Receipt {
  id: number;
  userId: number;
  membershipId?: number;
  recommendationId?: number;
  receiptDate: string;
  paymentAmount: number;
  receiptType: ReceiptType;
  payment?: Payment;
}

export interface Payment {
  id: number;
  receiptId: number;
  paymentDate: string;
}

export interface RecommendedMovie {
  recommendationId: number;
  movieId: number;
  possibleRating: number;
}

export interface UserMovie {
  userId: number;
  movieId: number;
  rating: number;
}