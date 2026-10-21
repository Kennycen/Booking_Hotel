import { useEffect, useState } from "react";
import toast from "react-hot-toast";
import Title from "../../components/Title";
import { useAppContext } from "../../context/AppContext";
import { errorMessage } from "../../lib/errorMessage";
import type { ApiResponse, Room } from "../../types";

export default function ListRoom() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const { axios, getToken, user, currency } = useAppContext();

  const loadRooms = () => {
    getToken()
      .then((token) =>
        axios.get<ApiResponse<{ rooms?: Array<Room & { id?: string }> }>>(
          "/api/rooms/owner",
          { headers: { Authorization: `Bearer ${token}` } },
        ),
      )
      .then(({ data }) => {
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
        toast.error(errorMessage(error, "Could not load rooms"));
      });
  };

  const toggleAvailability = (roomId: string) => {
    getToken()
      .then((token) =>
        axios.post<ApiResponse>(
          "/api/rooms/toggle-availability",
          { roomId },
          { headers: { Authorization: `Bearer ${token}` } },
        ),
      )
      .then(({ data }) => {
        if (data.success) {
          toast.success(data.message ?? "Room availability Updated");
          loadRooms();
        } else {
          toast.error(data.message ?? "Could not update room");
        }
      })
      .catch((error: unknown) => {
        toast.error(errorMessage(error, "Could not load rooms"));
      });
  };

  useEffect(() => {
    if (!user) return;
    let active = true;

    getToken()
      .then((token) =>
        axios.get<ApiResponse<{ rooms?: Array<Room & { id?: string }> }>>(
          "/api/rooms/owner",
          { headers: { Authorization: `Bearer ${token}` } },
        ),
      )
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
  }, [axios, getToken, user]);

  return (
    <div>
      <Title
        align="left"
        font="outfit"
        title="Room Listings"
        subTitle="View, edit, or manage all listed rooms. Keep the information up-to-date to provide the best experience for users."
      />
      <p className="text-gray-500 mt-8">All Rooms</p>
      <div className="w-full max-w-3xl text-left border border-gray-300 rounded-lg max-h-80 overflow-y-scroll mt-3">
        <table className="w-full">
          <thead className="bg-gray-50">
            <tr>
              <th className="py-3 px-4 text-gray-800 font-medium">Name</th>
              <th className="py-3 px-4 text-gray-800 font-medium max-sm:hidden">
                Facility
              </th>
              <th className="py-3 px-4 text-gray-800 font-medium">
                Price / night
              </th>
              <th className="py-3 px-4 text-gray-800 font-medium text-center">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="text-sm">
            {rooms.map((item) => (
              <tr key={item._id}>
                <td className="py-3 px-4 text-gray-700 border-t border-gray-300">
                  {item.roomType}
                </td>
                <td className="py-3 px-4 text-gray-700 border-t border-gray-300 max-sm:hidden">
                  {item.amenities.join(", ")}
                </td>
                <td className="py-3 px-4 text-gray-700 border-t border-gray-300">
                  {currency} {item.pricePerNight}
                </td>
                <td className="py-3 px-4 text-red-500 border-t text-center text-sm border-gray-300">
                  <label className="relative inline-flex items-center cursor-pointer text-gray-900 gap-3">
                    <input
                      type="checkbox"
                      onChange={() => void toggleAvailability(item._id)}
                      className="sr-only peer"
                      checked={item.isAvailable}
                    />
                    <div className="w-12 h-7 bg-slate-300 rounded-full peer peer-checked:bg-blue-600 transition-colors duration-200"></div>
                    <span className="dot absolute left-1 top-1 w-5 h-5 bg-white rounded-full transition-transform duration-200 ease-in-out peer-checked:translate-x-5"></span>
                  </label>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
