import { useAuth, useUser } from "@clerk/react";
import axios from "axios";
import {
  createContext,
  useContext,
  useEffect,
  useState,
  type Dispatch,
  type ReactNode,
  type SetStateAction,
} from "react";
import toast from "react-hot-toast";
import { useNavigate } from "react-router-dom";

axios.defaults.baseURL = import.meta.env.VITE_BACKEND_URL;

export type Room = {
  _id: string;
  images: string[];
  pricePerNight: number;
  hotel: {
    name: string;
    address: string;
    city: string;
  };
};

type AppContextValue = {
  currency: string;
  navigate: ReturnType<typeof useNavigate>;
  user: ReturnType<typeof useUser>["user"];
  getToken: ReturnType<typeof useAuth>["getToken"];
  isOwner: boolean;
  setIsOwner: (value: boolean) => void;
  axios: typeof axios;
  showHotelReg: boolean;
  setShowHotelReg: (value: boolean) => void;
  searchedCities: string[];
  setSearchedCities: Dispatch<SetStateAction<string[]>>;
  rooms: Room[];
  setRooms: Dispatch<SetStateAction<Room[]>>;
};

const AppContext = createContext<AppContextValue | null>(null);

export default function AppProvider({ children }: { children: ReactNode }) {
  const currency = import.meta.env.VITE_CURRENCY || "$";
  const navigate = useNavigate();
  const { user } = useUser();
  const { getToken } = useAuth();

  const [isOwner, setIsOwner] = useState(false);
  const [showHotelReg, setShowHotelReg] = useState(false);
  const [searchedCities, setSearchedCities] = useState<string[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);

  const fetchRooms = async () => {
    try {
      const { data } = await axios.get("/api/rooms");
      if (data.success) {
        setRooms(
          data.rooms.map((room: Room & { id?: string }) => ({
            ...room,
            _id: room._id ?? room.id ?? "",
          })),
        );
      } else {
        toast.error(data.message);
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not load rooms");
    }
  };

  const fetchUser = async () => {
    try {
      const { data } = await axios.get("/api/user", {
        headers: { Authorization: `Bearer ${await getToken()}` },
      });
      if (data.success) {
        setIsOwner(data.role === "hotelOwner");
        setSearchedCities(data.recentSearchedCities || []);
      }
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Could not load user");
    }
  };

  useEffect(() => {
    if (user) {
      fetchUser();
    }
  }, [user]);

  useEffect(() => {
    fetchRooms();
  }, []);

  const value: AppContextValue = {
    currency,
    navigate,
    user,
    getToken,
    isOwner,
    setIsOwner,
    axios,
    showHotelReg,
    setShowHotelReg,
    searchedCities,
    setSearchedCities,
    rooms,
    setRooms,
  };
  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error("useAppContext must be used within AppProvider");
  }
  return context;
}
