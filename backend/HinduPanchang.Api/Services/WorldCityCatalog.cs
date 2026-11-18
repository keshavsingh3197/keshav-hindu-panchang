using HinduPanchang.Api.Models;

namespace HinduPanchang.Api.Services;

/// <summary>
/// Built-in zero-latency catalog of sacred Indian tirthas, state capitals, and major global cities
/// complete with exact WGS84 coordinates and IANA TimeZone IDs for automatic historical DST.
/// </summary>
public static class WorldCityCatalog
{
    public static IReadOnlyList<PanchangCity> All { get; } =
    [
        // North, Central & Sacred Tirthas (India — Asia/Kolkata)
        C("in-del", "New Delhi, India", "नई दिल्ली (New Delhi)", "Delhi", "India", 28.6139, 77.2090, 216, "Asia/Kolkata", 5.5),
        C("in-vns", "Varanasi, India", "काशी / वाराणसी (Varanasi)", "Uttar Pradesh", "India", 25.3176, 82.9739, 80, "Asia/Kolkata", 5.5),
        C("in-ayo", "Ayodhya, India", "अयोध्या (Ayodhya)", "Uttar Pradesh", "India", 26.7922, 82.1998, 93, "Asia/Kolkata", 5.5),
        C("in-pry", "Prayagraj, India", "प्रयागराज (Prayagraj)", "Uttar Pradesh", "India", 25.4358, 81.8463, 98, "Asia/Kolkata", 5.5),
        C("in-mtr", "Mathura, India", "मथुरा (Mathura)", "Uttar Pradesh", "India", 27.4924, 77.6737, 174, "Asia/Kolkata", 5.5),
        C("in-vrn", "Vrindavan, India", "वृन्दावन (Vrindavan)", "Uttar Pradesh", "India", 27.5806, 77.7006, 170, "Asia/Kolkata", 5.5),
        C("in-hw", "Haridwar, India", "हरिद्वार (Haridwar)", "Uttarakhand", "India", 29.9457, 78.1642, 314, "Asia/Kolkata", 5.5),
        C("in-rsh", "Rishikesh, India", "ऋषिकेश (Rishikesh)", "Uttarakhand", "India", 30.0869, 78.2676, 372, "Asia/Kolkata", 5.5),
        C("in-kdr", "Kedarnath, India", "केदारनाथ (Kedarnath)", "Uttarakhand", "India", 30.7346, 79.0669, 3583, "Asia/Kolkata", 5.5),
        C("in-bdr", "Badrinath, India", "बद्रीनाथ (Badrinath)", "Uttarakhand", "India", 30.7433, 79.4938, 3100, "Asia/Kolkata", 5.5),
        C("in-ujj", "Ujjain, India", "उज्जैन (Ujjain)", "Madhya Pradesh", "India", 23.1765, 75.7885, 494, "Asia/Kolkata", 5.5),
        C("in-omk", "Omkareshwar, India", "ओंकारेश्वर (Omkareshwar)", "Madhya Pradesh", "India", 22.2455, 76.1509, 250, "Asia/Kolkata", 5.5),
        C("in-jai", "Jaipur, India", "जयपुर (Jaipur)", "Rajasthan", "India", 26.9124, 75.7873, 431, "Asia/Kolkata", 5.5),
        C("in-jdh", "Jodhpur, India", "जोधपुर (Jodhpur)", "Rajasthan", "India", 26.2389, 73.0243, 231, "Asia/Kolkata", 5.5),
        C("in-udr", "Udaipur, India", "उदयपुर (Udaipur)", "Rajasthan", "India", 24.5854, 73.7125, 598, "Asia/Kolkata", 5.5),
        C("in-ajm", "Ajmer, India", "अजमेर (Ajmer)", "Rajasthan", "India", 26.4499, 74.6399, 480, "Asia/Kolkata", 5.5),
        C("in-psh", "Pushkar, India", "पुष्कर (Pushkar)", "Rajasthan", "India", 26.4897, 74.5511, 510, "Asia/Kolkata", 5.5),
        C("in-kot", "Kota, India", "कोटा (Kota)", "Rajasthan", "India", 25.2138, 75.8648, 271, "Asia/Kolkata", 5.5),
        C("in-bkn", "Bikaner, India", "बीकानेर (Bikaner)", "Rajasthan", "India", 28.0229, 73.3119, 242, "Asia/Kolkata", 5.5),
        C("in-agr", "Agra, India", "आगरा (Agra)", "Uttar Pradesh", "India", 27.1767, 78.0081, 171, "Asia/Kolkata", 5.5),
        C("in-lko", "Lucknow, India", "लखनऊ (Lucknow)", "Uttar Pradesh", "India", 26.8467, 80.9462, 123, "Asia/Kolkata", 5.5),
        C("in-knp", "Kanpur, India", "कानपुर (Kanpur)", "Uttar Pradesh", "India", 26.4499, 80.3319, 126, "Asia/Kolkata", 5.5),
        C("in-gkp", "Gorakhpur, India", "गोरखपुर (Gorakhpur)", "Uttar Pradesh", "India", 26.7606, 83.3732, 84, "Asia/Kolkata", 5.5),
        C("in-noi", "Noida, India", "नोएडा (Noida)", "Uttar Pradesh", "India", 28.5355, 77.3910, 200, "Asia/Kolkata", 5.5),
        C("in-ggn", "Gurugram, India", "गुरुग्राम (Gurugram)", "Haryana", "India", 28.4595, 77.0266, 217, "Asia/Kolkata", 5.5),
        C("in-kkx", "Kurukshetra, India", "कुरुक्षेत्र (Kurukshetra)", "Haryana", "India", 29.9695, 76.8783, 259, "Asia/Kolkata", 5.5),
        C("in-chd", "Chandigarh, India", "चंडीगढ़ (Chandigarh)", "Chandigarh", "India", 30.7333, 76.7794, 321, "Asia/Kolkata", 5.5),
        C("in-asr", "Amritsar, India", "अमृतसर (Amritsar)", "Punjab", "India", 31.6340, 74.8723, 234, "Asia/Kolkata", 5.5),
        C("in-ldh", "Ludhiana, India", "लुधियाना (Ludhiana)", "Punjab", "India", 30.9010, 75.8573, 244, "Asia/Kolkata", 5.5),
        C("in-sml", "Shimla, India", "शिमला (Shimla)", "Himachal Pradesh", "India", 31.1048, 77.1734, 2276, "Asia/Kolkata", 5.5),
        C("in-ddn", "Dehradun, India", "देहरादून (Dehradun)", "Uttarakhand", "India", 30.3165, 78.0322, 640, "Asia/Kolkata", 5.5),
        C("in-ixj", "Jammu, India", "जम्मू (Jammu)", "Jammu & Kashmir", "India", 32.7266, 74.8570, 327, "Asia/Kolkata", 5.5),
        C("in-sxr", "Srinagar, India", "श्रीनगर (Srinagar)", "Jammu & Kashmir", "India", 34.0837, 74.7973, 1585, "Asia/Kolkata", 5.5),

        // West & South & East (India — Asia/Kolkata)
        C("in-bom", "Mumbai, India", "मुंबई (Mumbai)", "Maharashtra", "India", 19.0760, 72.8777, 14, "Asia/Kolkata", 5.5),
        C("in-pnq", "Pune, India", "पुणे (Pune)", "Maharashtra", "India", 18.5204, 73.8567, 560, "Asia/Kolkata", 5.5),
        C("in-nag", "Nagpur, India", "नागपुर (Nagpur)", "Maharashtra", "India", 21.1458, 79.0882, 310, "Asia/Kolkata", 5.5),
        C("in-nsk", "Nashik, India", "नासिक (Nashik)", "Maharashtra", "India", 19.9975, 73.7898, 584, "Asia/Kolkata", 5.5),
        C("in-shd", "Shirdi, India", "शिर्डी (Shirdi)", "Maharashtra", "India", 19.7645, 74.4762, 504, "Asia/Kolkata", 5.5),
        C("in-aur", "Chhatrapati Sambhajinagar, India", "छत्रपती संभाजीनगर", "Maharashtra", "India", 19.8762, 75.3433, 568, "Asia/Kolkata", 5.5),
        C("in-klh", "Kolhapur, India", "कोल्हापुर (Kolhapur)", "Maharashtra", "India", 16.7050, 74.2433, 545, "Asia/Kolkata", 5.5),
        C("in-amd", "Ahmedabad, India", "अहमदाबाद (Ahmedabad)", "Gujarat", "India", 23.0225, 72.5714, 53, "Asia/Kolkata", 5.5),
        C("in-srt", "Surat, India", "सूरत (Surat)", "Gujarat", "India", 21.1702, 72.8311, 13, "Asia/Kolkata", 5.5),
        C("in-bdq", "Vadodara, India", "वडोदरा (Vadodara)", "Gujarat", "India", 22.3072, 73.1812, 39, "Asia/Kolkata", 5.5),
        C("in-raj", "Rajkot, India", "राजकोट (Rajkot)", "Gujarat", "India", 22.3039, 70.8022, 128, "Asia/Kolkata", 5.5),
        C("in-dwk", "Dwarka, India", "द्वारका (Dwarka)", "Gujarat", "India", 22.2442, 68.9685, 11, "Asia/Kolkata", 5.5),
        C("in-som", "Somnath, India", "सोमनाथ (Somnath)", "Gujarat", "India", 20.8880, 70.4012, 10, "Asia/Kolkata", 5.5),
        C("in-idr", "Indore, India", "इंदौर (Indore)", "Madhya Pradesh", "India", 22.7196, 75.8577, 553, "Asia/Kolkata", 5.5),
        C("in-bpl", "Bhopal, India", "भोपाल (Bhopal)", "Madhya Pradesh", "India", 23.2599, 77.4126, 500, "Asia/Kolkata", 5.5),
        C("in-jbp", "Jabalpur, India", "जबलपुर (Jabalpur)", "Madhya Pradesh", "India", 23.1815, 79.9864, 412, "Asia/Kolkata", 5.5),
        C("in-gwl", "Gwalior, India", "ग्वालियर (Gwalior)", "Madhya Pradesh", "India", 26.2183, 78.1828, 197, "Asia/Kolkata", 5.5),
        C("in-rpr", "Raipur, India", "रायपुर (Raipur)", "Chhattisgarh", "India", 21.2514, 81.6296, 298, "Asia/Kolkata", 5.5),
        C("in-goa", "Panaji, India", "पणजी (Goa)", "Goa", "India", 15.4909, 73.8278, 7, "Asia/Kolkata", 5.5),
        C("in-blr", "Bengaluru, India", "बेंगलुरु (Bengaluru)", "Karnataka", "India", 12.9716, 77.5946, 920, "Asia/Kolkata", 5.5),
        C("in-mys", "Mysuru, India", "मैसूर (Mysuru)", "Karnataka", "India", 12.2958, 76.6394, 763, "Asia/Kolkata", 5.5),
        C("in-ixg", "Mangaluru, India", "मंगलुरु (Mangaluru)", "Karnataka", "India", 12.9141, 74.8560, 22, "Asia/Kolkata", 5.5),
        C("in-udpi", "Udupi, India", "उडुपी (Udupi)", "Karnataka", "India", 13.3409, 74.7421, 14, "Asia/Kolkata", 5.5),
        C("in-hyd", "Hyderabad, India", "हैदराबाद (Hyderabad)", "Telangana", "India", 17.3850, 78.4867, 505, "Asia/Kolkata", 5.5),
        C("in-vtz", "Visakhapatnam, India", "विशाखापत्तनम", "Andhra Pradesh", "India", 17.6868, 83.2185, 45, "Asia/Kolkata", 5.5),
        C("in-vja", "Vijayawada, India", "विजयवाड़ा (Vijayawada)", "Andhra Pradesh", "India", 16.5062, 80.6480, 23, "Asia/Kolkata", 5.5),
        C("in-tpt", "Tirupati, India", "तिरुपति (Tirupati)", "Andhra Pradesh", "India", 13.6288, 79.4192, 162, "Asia/Kolkata", 5.5),
        C("in-maa", "Chennai, India", "चेन्नई (Chennai)", "Tamil Nadu", "India", 13.0827, 80.2707, 6, "Asia/Kolkata", 5.5),
        C("in-ixm", "Madurai, India", "मदुरै (Madurai)", "Tamil Nadu", "India", 9.9252, 78.1198, 134, "Asia/Kolkata", 5.5),
        C("in-cjb", "Coimbatore, India", "कोयंबटूर (Coimbatore)", "Tamil Nadu", "India", 11.0168, 76.9558, 411, "Asia/Kolkata", 5.5),
        C("in-rmm", "Rameswaram, India", "रामेश्वरम (Rameswaram)", "Tamil Nadu", "India", 9.2876, 79.3129, 10, "Asia/Kolkata", 5.5),
        C("in-cok", "Kochi, India", "कोच्चि (Kochi)", "Kerala", "India", 9.9312, 76.2673, 0, "Asia/Kolkata", 5.5),
        C("in-trv", "Thiruvananthapuram, India", "तिरुवनंतपुरम", "Kerala", "India", 8.5241, 76.9366, 10, "Asia/Kolkata", 5.5),
        C("in-ccu", "Kolkata, India", "कोलकाता (Kolkata)", "West Bengal", "India", 22.5726, 88.3639, 9, "Asia/Kolkata", 5.5),
        C("in-pat", "Patna, India", "पटना (Patna)", "Bihar", "India", 25.5941, 85.1376, 53, "Asia/Kolkata", 5.5),
        C("in-gya", "Bodh Gaya, India", "बोधगया (Gaya)", "Bihar", "India", 24.6961, 84.9869, 111, "Asia/Kolkata", 5.5),
        C("in-ixr", "Ranchi, India", "रांची (Ranchi)", "Jharkhand", "India", 23.3441, 85.3096, 651, "Asia/Kolkata", 5.5),
        C("in-deo", "Deoghar, India", "देवघर (Baidyanath Dham)", "Jharkhand", "India", 24.4820, 86.6952, 254, "Asia/Kolkata", 5.5),
        C("in-bbi", "Bhubaneswar, India", "भुवनेश्वर (Bhubaneswar)", "Odisha", "India", 20.2961, 85.8245, 45, "Asia/Kolkata", 5.5),
        C("in-pur", "Puri, India", "जगन्नाथ पुरी (Puri)", "Odisha", "India", 19.8135, 85.8312, 0, "Asia/Kolkata", 5.5),
        C("in-gau", "Guwahati, India", "गुवाहाटी (Guwahati)", "Assam", "India", 26.1445, 91.7362, 55, "Asia/Kolkata", 5.5),
        C("in-gtg", "Gangtok, India", "गंगटोक (Gangtok)", "Sikkim", "India", 27.3389, 88.6065, 1650, "Asia/Kolkata", 5.5),
        C("in-shl", "Shillong, India", "शिलांग (Shillong)", "Meghalaya", "India", 25.5788, 91.8933, 1496, "Asia/Kolkata", 5.5),

        // Nepal, South Asia & Indo-Pacific
        C("np-ktm", "Kathmandu, Nepal", "काठमांडू (Kathmandu)", "Bagmati", "Nepal", 27.7172, 85.3240, 1400, "Asia/Kathmandu", 5.75),
        C("np-pkr", "Pokhara, Nepal", "पोखरा (Pokhara)", "Gandaki", "Nepal", 28.2096, 83.9856, 822, "Asia/Kathmandu", 5.75),
        C("np-jnk", "Janakpur, Nepal", "जनकपुर (Janakpur)", "Madhesh", "Nepal", 26.7288, 85.9263, 74, "Asia/Kathmandu", 5.75),
        C("lk-cmb", "Colombo, Sri Lanka", "कोलंबो (Colombo)", "Western", "Sri Lanka", 6.9271, 79.8612, 1, "Asia/Colombo", 5.5),
        C("bd-dac", "Dhaka, Bangladesh", "ढाका (Dhaka)", "Dhaka", "Bangladesh", 23.8103, 90.4125, 4, "Asia/Dhaka", 6.0),
        C("sg-sin", "Singapore", "सिंगापुर (Singapore)", "Central", "Singapore", 1.3521, 103.8198, 15, "Asia/Singapore", 8.0),
        C("my-kul", "Kuala Lumpur, Malaysia", "कुआलालंपुर", "Federal Territory", "Malaysia", 3.1390, 101.6869, 22, "Asia/Kuala_Lumpur", 8.0),
        C("th-bkk", "Bangkok, Thailand", "बैंकॉक (Bangkok)", "Bangkok", "Thailand", 13.7563, 100.5018, 2, "Asia/Bangkok", 7.0),
        C("id-dps", "Bali (Denpasar), Indonesia", "बाली (Denpasar)", "Bali", "Indonesia", -8.6705, 115.2126, 4, "Asia/Makassar", 8.0),
        C("jp-tyo", "Tokyo, Japan", "टोक्यो (Tokyo)", "Tokyo", "Japan", 35.6762, 139.6503, 40, "Asia/Tokyo", 9.0),
        C("au-syd", "Sydney, Australia", "सिडनी (Sydney)", "New South Wales", "Australia", -33.8688, 151.2093, 19, "Australia/Sydney", 10.0),
        C("au-mel", "Melbourne, Australia", "मेलबर्न (Melbourne)", "Victoria", "Australia", -37.8136, 144.9631, 31, "Australia/Melbourne", 10.0),
        C("nz-akl", "Auckland, New Zealand", "ऑकलैंड (Auckland)", "Auckland", "New Zealand", -36.8485, 174.7633, 26, "Pacific/Auckland", 12.0),
        C("fj-suv", "Suva, Fiji", "सुवा (Fiji)", "Central", "Fiji", -18.1248, 178.4501, 10, "Pacific/Fiji", 12.0),

        // Middle East, Europe & Africa
        C("ae-dxb", "Dubai, UAE", "दुबई (Dubai)", "Dubai", "UAE", 25.2048, 55.2708, 5, "Asia/Dubai", 4.0),
        C("ae-auh", "Abu Dhabi, UAE", "अबू धाबी (Abu Dhabi)", "Abu Dhabi", "UAE", 24.4539, 54.3773, 12, "Asia/Dubai", 4.0),
        C("qa-doh", "Doha, Qatar", "दोहा (Doha)", "Doha", "Qatar", 25.2854, 51.5310, 10, "Asia/Qatar", 3.0),
        C("mu-plu", "Port Louis, Mauritius", "पोर्ट लुइस (Mauritius)", "Port Louis", "Mauritius", -20.1609, 57.5012, 13, "Indian/Mauritius", 4.0),
        C("ke-nbo", "Nairobi, Kenya", "नैरोबी (Nairobi)", "Nairobi", "Kenya", -1.2921, 36.8219, 1795, "Africa/Nairobi", 3.0),
        C("za-jnb", "Johannesburg, South Africa", "जोहान्सबर्ग", "Gauteng", "South Africa", -26.2041, 28.0473, 1753, "Africa/Johannesburg", 2.0),
        C("gb-lon", "London, UK", "लंदन (London, UK)", "England", "UK", 51.5072, -0.1276, 11, "Europe/London", 0.0),
        C("gb-bhx", "Birmingham, UK", "बर्मिंघम (UK)", "England", "UK", 52.4862, -1.8904, 140, "Europe/London", 0.0),
        C("gb-lei", "Leicester, UK", "लीसेस्टर (UK)", "England", "UK", 52.6369, -1.1398, 65, "Europe/London", 0.0),
        C("fr-par", "Paris, France", "पेरिस (Paris)", "Ile-de-France", "France", 48.8566, 2.3522, 35, "Europe/Paris", 1.0),
        C("de-ber", "Berlin, Germany", "बर्लिन (Berlin)", "Berlin", "Germany", 52.5200, 13.4050, 34, "Europe/Berlin", 1.0),
        C("nl-ams", "Amsterdam, Netherlands", "एम्स्टर्डम", "North Holland", "Netherlands", 52.3676, 4.9041, 2, "Europe/Amsterdam", 1.0),
        C("ch-zrh", "Zurich, Switzerland", "ज़्यूरिख़ (Zurich)", "Zurich", "Switzerland", 47.3769, 8.5417, 408, "Europe/Zurich", 1.0),

        // Americas (Full DST-Aware IANA TimeZones)
        C("us-nyc", "New York City, USA", "न्यूयॉर्क (New York, USA)", "New York", "USA", 40.7128, -74.0060, 10, "America/New_York", -5.0),
        C("us-chi", "Chicago, USA", "शिकागो (Chicago)", "Illinois", "USA", 41.8781, -87.6298, 182, "America/Chicago", -6.0),
        C("us-hou", "Houston, USA", "ह्यूस्टन (Houston)", "Texas", "USA", 29.7604, -95.3698, 15, "America/Chicago", -6.0),
        C("us-dal", "Dallas, USA", "डलास (Dallas)", "Texas", "USA", 32.7767, -96.7970, 131, "America/Chicago", -6.0),
        C("us-atl", "Atlanta, USA", "अटलांटा (Atlanta)", "Georgia", "USA", 33.7490, -84.3880, 320, "America/New_York", -5.0),
        C("us-sfo", "San Francisco, USA", "सैन फ्रांसिस्को", "California", "USA", 37.7749, -122.4194, 16, "America/Los_Angeles", -8.0),
        C("us-sjc", "San Jose, USA", "सैन होज़े (Bay Area)", "California", "USA", 37.3382, -121.8863, 25, "America/Los_Angeles", -8.0),
        C("us-lax", "Los Angeles, USA", "लॉस एंजिल्स", "California", "USA", 34.0522, -118.2437, 89, "America/Los_Angeles", -8.0),
        C("us-sea", "Seattle, USA", "सिएटल (Seattle)", "Washington", "USA", 47.6062, -122.3321, 56, "America/Los_Angeles", -8.0),
        C("ca-yyz", "Toronto, Canada", "टोरंटो (Toronto)", "Ontario", "Canada", 43.6532, -79.3832, 76, "America/Toronto", -5.0),
        C("ca-yvr", "Vancouver, Canada", "वैंकूवर (Vancouver)", "British Columbia", "Canada", 49.2827, -123.1207, 2, "America/Vancouver", -8.0),
        C("tt-pos", "Port of Spain, Trinidad", "पोर्ट ऑफ़ स्पेन", "Trinidad", "Trinidad & Tobago", 10.6549, -61.5019, 10, "America/Port_of_Spain", -4.0),
        C("gy-geo", "Georgetown, Guyana", "जॉर्जटाउन (Guyana)", "Demerara", "Guyana", 6.8013, -58.1551, 0, "America/Guyana", -4.0),
        C("sr-pbm", "Paramaribo, Suriname", "पारामारिबो (Suriname)", "Paramaribo", "Suriname", 5.8520, -55.2038, 3, "America/Paramaribo", -3.0)
    ];

    private static PanchangCity C(
        string id,
        string name,
        string hindi,
        string state,
        string country,
        double lat,
        double lon,
        double elev,
        string tzId,
        double stdOffset) => new()
    {
        Id = id,
        Name = name,
        LabelHindi = hindi,
        State = state,
        Country = country,
        Latitude = lat,
        Longitude = lon,
        ElevationMeters = elev,
        TimeZoneId = tzId,
        TimezoneOffsetHours = stdOffset,
        UsesDst = tzId is not ("Asia/Kolkata" or "Asia/Kathmandu" or "Asia/Colombo" or "Asia/Dhaka"
            or "Asia/Dubai" or "Asia/Qatar" or "Asia/Singapore" or "Asia/Kuala_Lumpur" or "Asia/Bangkok"
            or "Asia/Makassar" or "Asia/Tokyo" or "Africa/Nairobi" or "Africa/Johannesburg" or "Indian/Mauritius"
            or "America/Port_of_Spain" or "America/Guyana" or "America/Paramaribo"),
        IsActive = true
    };
}
