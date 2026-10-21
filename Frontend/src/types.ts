export type HotelOwner = {
  image: string;
};

export type Hotel = {
  _id?: string;
  name: string;
  address: string;
  city: string;
  contact?: string;
  owner?: HotelOwner | null;
};

export type Room = {
  _id: string;
  roomType: string;
  images: string[];
  pricePerNight: number;
  amenities: string[];
  isAvailable: boolean;
  createdAt?: string;
  hotel: Hotel;
};

export type BookingRoom = {
  _id: string;
  roomType: string;
  pricePerNight: number;
  images: string[];
};

export type BookingHotel = {
  _id: string;
  name: string;
  address: string;
  city: string;
};

export type Booking = {
  _id: string;
  checkInDate: string;
  checkOutDate: string;
  totalPrice: number;
  guests: number;
  isPaid: boolean;
  paymentMethod: string;
  status: string;
  room: BookingRoom;
  hotel: BookingHotel;
};

export type DashboardBooking = {
  _id: string;
  totalPrice: number;
  isPaid: boolean;
  user: {
    _id: string;
    username: string;
  };
  room: {
    _id: string;
    roomType: string;
  };
};

export type DashboardData = {
  totalBookings: number;
  totalRevenue: number;
  bookings: DashboardBooking[];
};

export type ApiResponse<T extends object = object> = {
  success: boolean;
  message?: string;
} & T;
