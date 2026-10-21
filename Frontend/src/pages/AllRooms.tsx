import { useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { assets, facilityIcons } from "../assets/assets";
import StarRating from "../components/StarRating";
import { useAppContext, type Room } from "../context/AppContext";

type FacilityIcon = keyof typeof facilityIcons;

type RoomFilters = {
  roomType: string[];
  priceRange: string[];
};

type FilterKey = keyof RoomFilters;

const sortOptions = [
  "Price Low to High",
  "Price High to Low",
  "Newest First",
] as const;

type SortOption = (typeof sortOptions)[number];

const roomTypes = ["Single Bed", "Double Bed", "Luxury Bed", "Family Suite"];

const priceRanges = ["0 to 500", "500 to 1000", "1000 to 2000", "2000 to 3000"];

const emptyFilters: RoomFilters = {
  roomType: [],
  priceRange: [],
};

type CheckBoxProps = {
  label: string;
  selected: boolean;
  onChange: (checked: boolean, label: string) => void;
};

function CheckBox({ label, selected, onChange }: CheckBoxProps) {
  return (
    <label className="flex gap-3 items-center cursor-pointer mt-2 text-sm">
      <input
        type="checkbox"
        checked={selected}
        onChange={(event) => onChange(event.target.checked, label)}
      />
      <span className="font-light select-none">{label}</span>
    </label>
  );
}

type RadioButtonProps = {
  label: string;
  selected: boolean;
  onChange: (label: string) => void;
};

function RadioButton({ label, selected, onChange }: RadioButtonProps) {
  return (
    <label className="flex gap-3 items-center cursor-pointer mt-2 text-sm">
      <input
        type="radio"
        name="sortOption"
        checked={selected}
        onChange={() => onChange(label)}
      />
      <span className="font-light select-none">{label}</span>
    </label>
  );
}

function isFacilityIcon(name: string): name is FacilityIcon {
  return name in facilityIcons;
}

function createdAtTime(room: Room) {
  return room.createdAt ? new Date(room.createdAt).getTime() : 0;
}

export default function AllRooms() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { rooms, navigate, currency } = useAppContext();
  const destination = searchParams.get("destination");

  const [openFilters, setOpenFilters] = useState(false);
  const [selectedSort, setSelectedSort] = useState<SortOption | "">("");
  const [selectedFilters, setSelectedFilters] =
    useState<RoomFilters>(emptyFilters);

  const handleFilterChange = (
    checked: boolean,
    value: string,
    type: FilterKey,
  ) => {
    setSelectedFilters((prevFilters) => {
      const current = prevFilters[type];
      const nextValues = checked
        ? current.includes(value)
          ? current
          : [...current, value]
        : current.filter((item) => item !== value);

      return { ...prevFilters, [type]: nextValues };
    });
  };

  const handleSortChange = (option: SortOption) => {
    setSelectedSort(option);
  };

  const filteredRooms = useMemo(() => {
    const matchesRoomType = (room: Room) =>
      selectedFilters.roomType.length === 0 ||
      selectedFilters.roomType.includes(room.roomType);

    const matchesPriceRange = (room: Room) =>
      selectedFilters.priceRange.length === 0 ||
      selectedFilters.priceRange.some((range) => {
        const [min = 0, max = 0] = range.split(" to ").map(Number);
        return room.pricePerNight > min && room.pricePerNight <= max;
      });

    const matchesDestination = (room: Room) => {
      if (!destination) return true;
      return room.hotel.city.toLowerCase().includes(destination.toLowerCase());
    };

    const compareRooms = (a: Room, b: Room) => {
      if (selectedSort === "Price Low to High") {
        return a.pricePerNight - b.pricePerNight;
      }
      if (selectedSort === "Price High to Low") {
        return b.pricePerNight - a.pricePerNight;
      }
      if (selectedSort === "Newest First") {
        return createdAtTime(b) - createdAtTime(a);
      }
      return 0;
    };

    return rooms
      .filter(
        (room) =>
          matchesRoomType(room) &&
          matchesPriceRange(room) &&
          matchesDestination(room),
      )
      .sort(compareRooms);
  }, [rooms, selectedFilters, selectedSort, destination]);

  const clearFilters = () => {
    setSelectedFilters(emptyFilters);
    setSelectedSort("");
    setSearchParams({});
  };

  const openRoom = (roomId: string) => {
    navigate(`/rooms/${roomId}`);
    scrollTo(0, 0);
  };

  return (
    <div className="flex flex-col-reverse lg:flex-row items-start justify-between pt-28 md:pt-35 px-4 md:px-16 lg:px-24 xl:px-32 ">
      <div>
        <div className="flex flex-col items-start text-left">
          <h1 className="font-playfair text-4xl md:text-[40px]">Hotel Rooms</h1>
          <p className="text-sm md:text-base text-gray-500/90 mt-2 max-w-174">
            Take advantage of our limited-time offers and special packages to
            enhance your stay and create unforgettable memories.
          </p>
        </div>

        {filteredRooms.map((room) => (
          <div
            key={room._id}
            className="flex flex-col md:flex-row items-start py-10 gap-6 border-b border-gray-300 last:pb-30 last:border-0"
          >
            <img
              onClick={() => openRoom(room._id)}
              src={room.images[0]}
              alt="hotel-img"
              title="View Room Details"
              className="max-h-65 md:w-1/2 rounded-xl shadow-lg object-cover cursor-pointer"
            />
            <div className="md:w-1/2 flex flex-col gap-2">
              <p className="text-gray-500">{room.hotel.city}</p>
              <p
                onClick={() => openRoom(room._id)}
                className="text-gray-800 text-3xl font-playfair"
              >
                {room.hotel.name}
              </p>
              <div className="flex items-center">
                <StarRating />
                <p className="ml-2">200+ reviews</p>
              </div>
              <div className="flex items-center gap-1 text-gray-500 mt-2 text-sm">
                <img src={assets.locationIcon} alt="location-icon" />
                <span>{room.hotel.address}</span>
              </div>
              <div className="flex flex-wrap items-center mt-3 mb-6 gap-4">
                {room.amenities.map((item) => (
                  <div
                    key={item}
                    className="flex items-center gap-2 px-3 py-2 rounded-lg bg-[#F5F5FF]/70"
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
              <p className="text-xl font-medium text-gray-700">
                ${room.pricePerNight} /night
              </p>
            </div>
          </div>
        ))}
      </div>
      <div className="bg-white w-80 border border-gray-300 text-gray-600 max-lg:mb-8 lg:mt-16">
        <div
          className={`flex items-center justify-between px-5 py-2.5 lg:border-b border-gray-300 ${
            openFilters ? "border-b" : ""
          }`}
        >
          <p className="text-base font-medium text-gray-800">FILTERS</p>
          <div className="text-xs cursor-pointer">
            <span
              onClick={() => setOpenFilters(!openFilters)}
              className="lg:hidden"
            >
              {openFilters ? "HIDE" : "SHOW"}
            </span>
            <span onClick={clearFilters} className="hidden lg:block">
              CLEAR
            </span>
          </div>
        </div>

        <div
          className={`${
            openFilters ? "h-auto" : "h-0 lg:h-auto"
          } overflow-hidden transition-all duration-700`}
        >
          <div className="px-5 pt-5">
            <p className="font-medium text-gray-800 pb-2">Popular filters</p>
            {roomTypes.map((room) => (
              <CheckBox
                key={room}
                label={room}
                selected={selectedFilters.roomType.includes(room)}
                onChange={(checked) =>
                  handleFilterChange(checked, room, "roomType")
                }
              />
            ))}
          </div>
          <div className="px-5 pt-5">
            <p className="font-medium text-gray-800 pb-2">Price Range</p>
            {priceRanges.map((range) => (
              <CheckBox
                key={range}
                label={`${currency} ${range}`}
                selected={selectedFilters.priceRange.includes(range)}
                onChange={(checked) =>
                  handleFilterChange(checked, range, "priceRange")
                }
              />
            ))}
          </div>
          <div className="px-5 pt-5 pb-7">
            <p className="font-medium text-gray-800 pb-2">Price Range</p>
            {sortOptions.map((option) => (
              <RadioButton
                key={option}
                label={option}
                selected={selectedSort === option}
                onChange={() => handleSortChange(option)}
              />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
