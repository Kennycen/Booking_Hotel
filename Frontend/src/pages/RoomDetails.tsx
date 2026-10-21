import { useState, type SubmitEvent } from "react";
import toast from "react-hot-toast";
import { useParams } from "react-router-dom";
import { assets, facilityIcons, roomCommonData } from "../assets/assets";
import StarRating from "../components/StarRating";
import { useAppContext } from "../context/AppContext";
import { errorMessage } from "../lib/errorMessage";
import type { ApiResponse } from "../types";

type FacilityIcon = keyof typeof facilityIcons;

function isFacilityIcon(name: string): name is FacilityIcon {
  return name in facilityIcons;
}

export default function RoomDetails() {
  const { id } = useParams();
  const { rooms, getToken, axios, navigate, currency } = useAppContext();
  const room = rooms.find((item) => item._id === id) ?? null;
  const [selectedImage, setSelectedImage] = useState<{
    roomId: string;
    image: string;
  } | null>(null);
  const mainImage =
    room && selectedImage?.roomId === room._id
      ? selectedImage.image
      : (room?.images[0] ?? null);
  const [checkInDate, setCheckInDate] = useState("");
  const [checkOutDate, setCheckOutDate] = useState("");
  const [guests, setGuests] = useState(1);
  const [isAvailable, setIsAvailable] = useState(false);

  const checkAvailability = async () => {
    try {
      if (checkInDate >= checkOutDate) {
        toast.error("Check-In Date should be less than Check-Out Date");
        return;
      }
      const { data } = await axios.post<ApiResponse<{ isAvailable?: boolean }>>(
        "/api/bookings/check-availability",
        {
          room: id,
          checkInDate,
          checkOutDate,
        },
      );
      if (data.success) {
        if (data.isAvailable) {
          setIsAvailable(true);
          toast.success("Room is available");
        } else {
          setIsAvailable(false);
          toast.error("Room is not available");
        }
      } else {
        toast.error(data.message ?? "Could not check availability");
      }
    } catch (error) {
      toast.error(errorMessage(error, "Could not check availability"));
    }
  };

  const onSubmitHandler = async (event: SubmitEvent<HTMLFormElement>) => {
    event.preventDefault();
    try {
      if (!isAvailable) {
        await checkAvailability();
        return;
      }

      const { data } = await axios.post<ApiResponse>(
        "/api/bookings/book",
        {
          room: id,
          checkInDate,
          checkOutDate,
          guests,
          paymentMethod: "Pay At Hotel",
        },
        { headers: { Authorization: `Bearer ${await getToken()}` } },
      );
      if (data.success) {
        toast.success(data.message ?? "Booking created successfully");
        navigate("/my-bookings");
        scrollTo(0, 0);
      } else {
        toast.error(data.message ?? "Could not book room");
      }
    } catch (error) {
      toast.error(errorMessage(error, "Could not book room"));
    }
  };

  if (!room || !mainImage) return null;

  return (
    <div className="py-28 md:py-35 px-4 md:px-16 lg:px-24 xl:px-32">
      <div className="flex flex-col md:flex-row items-start md:items-center gap-2">
        <h1 className="text-3xl md:text-4xl font-playfair">
          {room.hotel.name}{" "}
          <span className="font-inter text-sm">({room.roomType})</span>
        </h1>
        <p className="text-xs font-inter py-1.5 px-3 text-white bg-orange-500 rounded-full">
          20% OFF
        </p>
      </div>
      <div className="flex items-center gap-1 mt-2">
        <StarRating />
        <p className="ml-2">200+ reviews</p>
      </div>
      <div className="flex items-center gap-1 text-gray-500 mt-2">
        <img src={assets.locationIcon} alt="location-icon" />
        <span>{room.hotel.address}</span>
      </div>
      <div className="flex flex-col lg:flex-row mt-6 gap-6">
        <div className="lg:w-1/2 w-full">
          <img
            src={mainImage}
            alt="Room Image"
            className="w-full rounded-xl object-cover"
          />
        </div>
        <div className="grid grid-cols-2 gap-4 lg:w-1/2 w-full">
          {room.images.length > 1 &&
            room.images.map((image) => (
              <img
                onClick={() => {
                  if (!id) return;
                  setSelectedImage({ roomId: id, image });
                }}
                key={image}
                src={image}
                alt="Room Image"
                className={`w-full rounded-xl shadow-md object-cover cursor-pointer ${
                  mainImage === image ? "outline-3 outline-orange-500" : ""
                }`}
              />
            ))}
        </div>
      </div>

      <div className="flex flex-col md:flex-row md:justify-between mt-10">
        <div className="flex flex-col">
          <h1 className="text-3xl md:text-4xl font-playfair">
            Experience Luxury Like Never Before
          </h1>
          <div className="flex flex-wrap items-center mt-3 mb-6 gap-4">
            {room.amenities.map((item) => (
              <div
                key={item}
                className="flex items-center gap-2 px-3 py-2 rounded-lg bg-gray-100"
              >
                {isFacilityIcon(item) && (
                  <img
                    src={facilityIcons[item]}
                    alt={item}
                    className="w-5 h-5"
                  />
                )}
                <p className="text-xs">{item}</p>
              </div>
            ))}
          </div>
        </div>
        <p className="text-2xl font-medium">
          {currency}
          {room.pricePerNight}/night
        </p>
      </div>

      <form
        onSubmit={onSubmitHandler}
        className="flex flex-col md:flex-row items-start md:items-center justify-between bg-white shadow-[0px_0px_20px_rgba(0,0,0,0.15)] p-6 rounded-xl mx-auto mt-16 max-w-6xl"
      >
        <div className="flex flex-col flex-wrap md:flex-row items-start md:items-center gap-4 md:gap-10 text-gray-500">
          <div className="flex flex-col">
            <label htmlFor="checkInDate" className="font-medium">
              Check-In
            </label>
            <input
              onChange={(event) => {
                setCheckInDate(event.target.value);
                setIsAvailable(false);
              }}
              min={new Date().toISOString().split("T")[0]}
              type="date"
              id="checkInDate"
              value={checkInDate}
              className="w-full rounded border border-gray-300 px-3 py-2 mt-1.5 outline-none"
              required
            />
          </div>
          <div className="w-px h-15 bg-gray-300/70 max-md:hidden"></div>
          <div className="flex flex-col">
            <label htmlFor="checkOutDate" className="font-medium">
              Check-Out
            </label>
            <input
              onChange={(event) => {
                setCheckOutDate(event.target.value);
                setIsAvailable(false);
              }}
              min={checkInDate}
              disabled={!checkInDate}
              type="date"
              id="checkOutDate"
              value={checkOutDate}
              className="w-full rounded border border-gray-300 px-3 py-2 mt-1.5 outline-none"
              required
            />
          </div>
          <div className="w-px h-15 bg-gray-300/70 max-md:hidden"></div>
          <div className="flex flex-col">
            <label htmlFor="guests" className="font-medium">
              Guests
            </label>
            <input
              onChange={(event) => setGuests(Number(event.target.value))}
              value={guests}
              type="number"
              id="guests"
              min={1}
              className="max-w-20 rounded border border-gray-300 px-3 py-2 mt-1.5 outline-none"
              required
            />
          </div>
        </div>
        <button
          type="submit"
          className="bg-primary hover:bg-primary-dull active:scale-95 transition-all text-white rounded-md max-md:w-full max-md:mt-6 md:px-25 py-3 md:py-4 text-base cursor-pointer"
        >
          {isAvailable ? "Book Now" : "Check Availability"}
        </button>
      </form>

      <div className="mt-25 space-y-4">
        {roomCommonData.map((spec) => (
          <div key={spec.title} className="flex items-start gap-2">
            <img src={spec.icon} alt={`${spec.title}-icon`} className="w-6.5" />
            <div>
              <p className="text-base">{spec.title}</p>
              <p className="text-gray-500">{spec.description}</p>
            </div>
          </div>
        ))}
      </div>

      <div className="max-w-3xl border-y border-gray-300 my-15 py-10 text-gray-500">
        <p>
          Guests will be allocated on the ground floor according to
          Availability. You get a comfortable Two bedroom apartment has a true
          city feeling. The price quoted is for two guests, at the guest slot
          please mark the number of guests to get the exact price for groups.
          The Guests will be allocated ground floor according to availability.
          You get the comfortable two bedroom apartment that has a true city
          feeling.
        </p>
      </div>
      <div className="flex flex-col items-start gap-4">
        <div className="flex gap-4">
          {room.hotel.owner?.image && (
            <img
              src={room.hotel.owner.image}
              alt="Host"
              className="h-14 w-14 md:h-18 md:w-18 rounded-full"
            />
          )}
          <div>
            <p className="text-lg md:text-xl">Hosted by {room.hotel.name}</p>
            <div className="flex items-center mt-1">
              <StarRating />
              <p className="ml-2">200+ reviews</p>
            </div>
          </div>
        </div>
      </div>
      <button className="px-6 py-2.5 mt-4 rounded text-white bg-primary hover:bg-primary-dull transition-all cursor-pointer">
        Contact Now
      </button>
    </div>
  );
}
