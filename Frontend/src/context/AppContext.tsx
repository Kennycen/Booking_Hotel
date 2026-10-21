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
import { errorMessage } from "../lib/errorMessage";
import type { ApiResponse, Room } from "../types";

export type { Room };

axios.defaults.baseURL = import.meta.env.VITE_BACKEND_URL;

type AppContextValue = {
  currency: string;
  navigate: ReturnType<typeof useNavigate>;
  user: ReturnType<typeof useUser>["user"];
  authLoaded: boolean;
  userReady: boolean;
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
  const { user, isLoaded } = useUser();
  const { getToken } = useAuth();

  const [ownerFlag, setOwnerFlag] = useState(false);
  const [ownerForUserId, setOwnerForUserId] = useState<string | null>(null);
  const [showHotelReg, setShowHotelReg] = useState(false);
  const [searchedCities, setSearchedCities] = useState<string[]>([]);
  const [rooms, setRooms] = useState<Room[]>([]);

  const userReady = isLoaded && (!user || ownerForUserId === user.id);
  const isOwner = Boolean(user && ownerForUserId === user.id && ownerFlag);

  const setIsOwner = (value: boolean) => {
    if (!user) return;
    setOwnerFlag(value);
    setOwnerForUserId(user.id);
  };

  useEffect(() => {
    let active = true;

    axios
      .get<ApiResponse<{ rooms?: Array<Room & { id?: string }> }>>("/api/rooms")
      .then(({ data }) => {
        if (!active) return;
        if (data.success) {
          setRooms(
            (data.rooms ?? []).map((room) => ({
              ...room,
              _id: room._id || room.id || "",
            })),
          );
        } else {
          toast.error(data.message ?? "Could not load rooms");
        }
      })
      .catch((error: unknown) => {
        if (!active) return;
        toast.error(errorMessage(error, "Could not load rooms"));
      });

    return () => {
      active = false;
    };
  }, []);

  useEffect(() => {
    if (!isLoaded || !user) return;
    const userId = user.id;
    let active = true;

    getToken()
      .then((token) =>
        axios.get<
          ApiResponse<{ role?: string; recentSearchedCities?: string[] }>
        >("/api/user", {
          headers: { Authorization: `Bearer ${token}` },
        }),
      )
      .then(({ data }) => {
        if (!active) return;
        if (data.success) {
          setOwnerFlag(data.role === "hotelOwner");
          setSearchedCities(data.recentSearchedCities ?? []);
        } else {
          setOwnerFlag(false);
        }
        setOwnerForUserId(userId);
      })
      .catch((error: unknown) => {
        if (!active) return;
        setOwnerFlag(false);
        setOwnerForUserId(userId);
        toast.error(errorMessage(error, "Could not load user"));
      });

    return () => {
      active = false;
    };
  }, [getToken, isLoaded, user]);

  const value: AppContextValue = {
    currency,
    navigate,
    user,
    authLoaded: isLoaded,
    userReady,
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
