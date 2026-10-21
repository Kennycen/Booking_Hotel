import { Toaster } from "react-hot-toast";
import { Route, Routes, useLocation } from "react-router-dom";
import Footer from "./components/Footer";
import HotelReg from "./components/HotelReg";
import Navbar from "./components/Navbar";
import { useAppContext } from "./context/AppContext";
import AllRooms from "./pages/AllRooms";
import Home from "./pages/Home";
import MyBookings from "./pages/MyBookings";
import RoomDetails from "./pages/RoomDetails";
import AddRoom from "./pages/hotelOwner/AddRoom";
import Dashboard from "./pages/hotelOwner/Dashboard";
import Layout from "./pages/hotelOwner/Layout";
import ListRoom from "./pages/hotelOwner/ListRoom";

export default function App() {
  const isOwnerPath = useLocation().pathname.includes("owner");
  const { showHotelReg } = useAppContext();

  return (
    <>
      <Toaster />
      {!isOwnerPath && <Navbar />}
      {showHotelReg && <HotelReg />}
      <div className="min-h-[70vh]">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/rooms" element={<AllRooms />} />
          <Route path="/rooms/:id" element={<RoomDetails />} />
          <Route path="/my-bookings" element={<MyBookings />} />
          <Route path="/owner" element={<Layout />}>
            <Route index element={<Dashboard />} />
            <Route path="add-room" element={<AddRoom />} />
            <Route path="list-room" element={<ListRoom />} />
          </Route>
        </Routes>
      </div>
      <Footer />
    </>
  );
}
