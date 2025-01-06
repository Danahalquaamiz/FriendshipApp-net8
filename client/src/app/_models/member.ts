import { Photo } from "./Photo"

export interface Member {
  id: number
  userName: string
  age: number
  photoUrl: string
  created: Date
  lastActive: Date
  gender: string
  knownAs: String
  introduction: string
  interests: string
  lookingFor: string
  city: string
  country: string
  photos: Photo[]
}
